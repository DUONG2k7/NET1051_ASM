(function () {
    const INIT_KEY = '__orderNotificationsInitialized';

    function normalizeStatus(value) {
        if (typeof value === 'undefined' || value === null) {
            return '';
        }

        return value.toString().trim().toLowerCase();
    }

    function start() {
        const body = document.body;
        if (!body) {
            return;
        }

        const tableCode = body.dataset ? body.dataset.tableCode : null;
        if (!tableCode || window[INIT_KEY]) {
            return;
        }

        window[INIT_KEY] = true;

        const statusMessages = {
            pending: 'Chúng tôi đã nhận được đơn hàng của bạn.',
            confirmed: 'Đơn hàng của bạn đã được nhà bếp xác nhận.',
            in_kitchen: 'Đơn hàng của bạn đang được chế biến.',
            ready: 'Đơn hàng của bạn đã sẵn sàng, vui lòng nhận món.',
            served: 'Đơn hàng của bạn đã được phục vụ!',
            requested_bill: 'Nhà hàng đang chuẩn bị hóa đơn cho bạn.',
            paid: 'Đơn hàng đã thanh toán thành công. Cảm ơn bạn!',
            canceled: 'Đơn hàng của bạn đã bị huỷ.',
            cancelled: 'Đơn hàng của bạn đã bị huỷ.'
        };

        const statusTypes = {
            pending: 'info',
            confirmed: 'info',
            in_kitchen: 'info',
            ready: 'success',
            served: 'success',
            requested_bill: 'info',
            paid: 'success',
            canceled: 'error',
            cancelled: 'error'
        };

        const statusStorageKey = `site:orderStatus:${tableCode}`;

        function loadStatusCache() {
            try {
                const raw = sessionStorage.getItem(statusStorageKey);
                if (!raw) {
                    return {};
                }

                const parsed = JSON.parse(raw);
                if (!parsed || typeof parsed !== 'object') {
                    return {};
                }

                const sanitized = {};
                Object.keys(parsed).forEach(key => {
                    const value = parsed[key];
                    if (typeof value === 'string' && value) {
                        sanitized[key] = value;
                    }
                });

                return sanitized;
            } catch {
                return {};
            }
        }

        let orderStatusCache = loadStatusCache();

        function saveStatusCache(nextCache) {
            orderStatusCache = nextCache;
            try {
                sessionStorage.setItem(statusStorageKey, JSON.stringify(orderStatusCache));
            } catch {
                // ignore storage errors
            }
        }

        async function hydrateStatuses() {
            if (!window.fetch) {
                return;
            }

            const endpoints = [
                `/cart/status/list?tableCode=${encodeURIComponent(tableCode)}`,
                `/cart/my-orders?tableCode=${encodeURIComponent(tableCode)}`
            ];

            for (const endpoint of endpoints) {
                try {
                    const response = await fetch(endpoint, { cache: 'no-store' });
                    if (!response.ok) {
                        continue;
                    }

                    const data = await response.json();
                    if (!Array.isArray(data)) {
                        continue;
                    }

                    const nextCache = { ...orderStatusCache };
                    let hasChanges = false;

                    data.forEach(item => {
                        if (!item || typeof item !== 'object') {
                            return;
                        }

                        const id = item.id;
                        if (typeof id === 'undefined' || id === null) {
                            return;
                        }

                        const status = normalizeStatus(item.status);
                        if (!status) {
                            return;
                        }

                        const key = String(id);
                        if (nextCache[key] !== status) {
                            nextCache[key] = status;
                            hasChanges = true;
                        }
                    });

                    if (hasChanges) {
                        saveStatusCache(nextCache);
                    }

                    break;
                } catch {
                    // try next endpoint
                }
            }
        }

        function handleOrderUpdate(payload) {
            if (!payload || typeof payload !== 'object') {
                return;
            }

            const idValue = payload.id;
            const statusValue = payload.status;

            if (typeof idValue === 'undefined' || idValue === null) {
                return;
            }

            const normalizedStatus = normalizeStatus(statusValue);
            if (!normalizedStatus) {
                return;
            }

            const orderId = String(idValue);
            const previousStatus = orderStatusCache[orderId];

            if (previousStatus === normalizedStatus) {
                return;
            }

            const nextCache = { ...orderStatusCache };
            nextCache[orderId] = normalizedStatus;

            saveStatusCache(nextCache);

            const message = statusMessages[normalizedStatus];
            if (!message || typeof window.showNotification !== 'function') {
                return;
            }

            const type = statusTypes[normalizedStatus] || 'info';
            window.showNotification(message, type, {
                persist: true,
                duration: 5000
            });
        }

        function startConnection() {
            if (typeof signalR === 'undefined') {
                return;
            }

            let connection = null;

            const connect = () => {
                if (connection) {
                    return;
                }

                connection = new signalR.HubConnectionBuilder()
                    .withUrl('/hubs/order-status')
                    .withAutomaticReconnect()
                    .build();

                connection.on('OrderUpdated', handleOrderUpdate);
                connection.onreconnected(() => {
                    if (connection && tableCode) {
                        connection.invoke('JoinTable', tableCode).catch(() => { });
                    }
                    hydrateStatuses();
                });

                connection.onclose(() => {
                    connection = null;
                    setTimeout(connect, 5000);
                });

                connection.start()
                    .then(() => {
                        if (tableCode) {
                            connection.invoke('JoinTable', tableCode).catch(() => { });
                        }
                    })
                    .catch(() => {
                        connection = null;
                        setTimeout(connect, 5000);
                    });
            };

            connect();
        }

        hydrateStatuses();
        startConnection();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', start);
    } else {
        start();
    }
})();
