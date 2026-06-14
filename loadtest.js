import http from 'k6/http';
import { check, sleep, group } from 'k6';

export const options = {
    summaryTrendStats: ['avg', 'min', 'med', 'max', 'p(90)', 'p(95)', 'p(99)'],
    stages: [
        { duration: '30s', target: 50 },
        // Uncomment these to scale up to Breakpoint once 50 works perfectly!
        // { duration: '30s', target: 100 },
        // { duration: '30s', target: 150 },
        // { duration: '30s', target: 200 },
        // { duration: '30s', target: 250 },
        // { duration: '30s', target: 300 },
        // { duration: '30s', target: 350 },
        // { duration: '30s', target: 400 },
        // { duration: '30s', target: 450 },
        // { duration: '30s', target: 500 },
        // { duration: '30s', target: 550 },
        // { duration: '30s', target: 600 },
        // { duration: '30s', target: 650 },
        // { duration: '30s', target: 700 }, // Peak Stress Test Limit
        { duration: '60s', target: 0 },   // COOLDOWN PHASE: Allow Schedule worker to clean up
    ],
    thresholds: {
        http_req_failed: ['rate<0.15'],
    },
};
// This runs ONCE at the start of the test
export function setup() {
   const maxVUs = Math.max(...options.stages.map(s => s.target));
    
    console.log(`==============================================================`);
    console.log(`STRESS TEST START: ${new Date().toLocaleString('id-ID')}`);
    console.log(`Targeting ${maxVUs} Virtual Users...`); 
    console.log(`==============================================================`);
}

const BASE_URL = 'http://localhost';

export default function () {
    let fetchedTickets = [];
    let transactionNo = null;
    let expectedPrice = 0;

    group('1. View Tickets', function () {
        const resView = http.get(`${BASE_URL}/api/inventory/v1/Ticket`);

        check(resView, { 'View - Status is 200': (r) => r.status === 200 });

        if (resView.status === 200) {
            let body = JSON.parse(resView.body);
            if (body.length > 0) fetchedTickets = body;
        }
    });

    sleep(Math.random() * 1);

    if (fetchedTickets.length > 0) {
        group('2. Place Order', function () {

            const randomTicket = fetchedTickets[Math.floor(Math.random() * fetchedTickets.length)];

            // Capture the price just in case, though we will poll for the final amount later
            expectedPrice = randomTicket.price;

            const orderPayload = JSON.stringify({
                email: `student_${__VU}_${__ITER}@student.ub.ac.id`,
                categoryCode: randomTicket.ticketClassCode,
                quantity: 1
            });

            const params = { headers: { 'Content-Type': 'application/json' } };
            const resOrder = http.post(`${BASE_URL}/api/order/v1/Order/PlaceOrder`, orderPayload, params);

            check(resOrder, { 'Order - Status is 200': (r) => r.status === 200 });

            if (resOrder.status === 200) {
                let orderBody = JSON.parse(resOrder.body);
                transactionNo = orderBody.trxNo;
            } else {
                console.log(`[ORDER ERROR] Status: ${resOrder.status} | Sent Category: ${randomTicket.ticketClassCode} | Response: ${resOrder.body}`);
            }
        });
    }

    // 2.5 THE POLLING SPINNER: Wait for RabbitMQ to finish!
    if (transactionNo !== null) {
        let finalPrice = 0;
        let isReadyToPay = false;
        let attempts = 0;

        group('2.5 Polling Order Status', function () {
            // Loop up to 10 times (5 seconds max) waiting for the inventory service to finish
            while (attempts < 10 && !isReadyToPay) {
                sleep(0.5); // "Spinner" delay
                attempts++;

                // Hitting your exact updated controller endpoint
                const resStatus = http.get(`${BASE_URL}/api/order/v1/Order/GetOrderByOrderNo/${transactionNo}`);

                if (resStatus.status === 200) {
                    let orderData = JSON.parse(resStatus.body);

                    // Once RabbitMQ finishes, the status updates to WAITING_PAYMENT!
                    if (orderData.orderStatus === "WAITING_PAYMENT") {
                        isReadyToPay = true;
                        finalPrice = orderData.totalAmount; // Capture the final synchronized price!
                    }
                }
            }
        });

        // Now we branch into Pay or Cancel, completely dynamically!
        if (isReadyToPay) {
            const isAbandoningCart = (__ITER % 3 === 0);

            if (isAbandoningCart) {
                group('3. Cancel Order', function () {
                    const cancelPayload = JSON.stringify({ orderNo: transactionNo });
                    const params = { headers: { 'Content-Type': 'application/json' } };
                    const resCancel = http.post(`${BASE_URL}/api/order/v1/Order/CancelOrder`, cancelPayload, params);
                    check(resCancel, { 'Cancel - Status is 200': (r) => r.status === 200 });
                });
            } else {
                group('3. Pay Order', function () {
                    // We use the exact price fetched from your new GET endpoint
                    const payPayload = JSON.stringify({
                        orderNo: transactionNo,
                        paidAmount: finalPrice
                    });

                    const params = { headers: { 'Content-Type': 'application/json' } };
                    const resPay = http.post(`${BASE_URL}/api/order/v1/Order/OrderPayment`, payPayload, params);

                    check(resPay, { 'Pay - Status is 200': (r) => r.status === 200 });

                    if (resPay.status !== 200) {
                        console.log(`[PAY ERROR] Trx: ${transactionNo} | Server said: ${resPay.body}`);
                    }
                });
            }
        } else {
           check(null, { 'Order - Sync Timeout (Error)': (r) => false }); 
            console.log(`[POLL TIMEOUT] Order ${transactionNo} failed to sync within 5s.`);
        }
    }
}

export function teardown() {
    console.log(`==============================================================`);
    console.log(`STRESS TEST END: ${new Date().toLocaleString('id-ID')}`);
    console.log(`Tier test complete. Resetting environment for next run...`);
    console.log(`==============================================================`);
}