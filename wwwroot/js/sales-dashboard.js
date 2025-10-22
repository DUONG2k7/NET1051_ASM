// Sales Dashboard functionality
document.addEventListener('DOMContentLoaded', function () {
    initializeSalesDashboard();
    updateCurrentTime();
    setInterval(updateCurrentTime, 1000);
    loadPendingOrders();
    initializePerformanceChart();
});

// Initialize dashboard
function initializeSalesDashboard() {
    console.log('Sales Dashboard initialized');
    setupEventListeners();
}

// Update current time
function updateCurrentTime() {
    const now = new Date();
    const timeString = now.toLocaleTimeString('vi-VN', {
        hour12: false,
        hour: '2-digit',
        minute: '2-digit',
        second: '2-digit'
    });

    const timeElement = document.getElementById('currentTime');
    if (timeElement) {
        timeElement.textContent = timeString;
    }
}

// Setup event listeners
function setupEventListeners() {
    // Auto refresh pending orders every 30 seconds
    setInterval(loadPendingOrders, 30000);
}

// Load pending orders
function loadPendingOrders() {
    // TODO: Fetch from API
    console.log('Loading pending orders...');
}

// Dashboard actions
function refreshDashboard() {
    const refreshBtn = document.querySelector('.btn-refresh');
    const icon = refreshBtn.querySelector('i');

    icon.style.animation = 'spin 1s linear infinite';
    refreshBtn.disabled = true;

    setTimeout(() => {
        icon.style.animation = '';
        refreshBtn.disabled = false;
        showSalesNotification('Đã làm mới dữ liệu dashboard', 'success');
        loadPendingOrders();
    }, 1500);
}

// Order actions
function acceptOrder(orderId) {
    if (confirm(`Xác nhận đơn hàng ${orderId}?`)) {
        showSalesNotification(`Đã xác nhận đơn hàng ${orderId}`, 'success');
        // TODO: Call API to accept order
        removeOrderFromList(orderId);
    }
}

function callCustomer(phoneNumber) {
    // Simulate calling customer
    showSalesNotification(`Đang gọi ${phoneNumber}...`, 'info');

    // For desktop app integration or phone system
    if (navigator.userAgent.includes('Electron')) {
        // Electron app can integrate with phone system
        window.electronAPI?.makeCall(phoneNumber);
    } else {
        // Web version - open tel: link
        window.location.href = `tel:${phoneNumber}`;
    }
}

function viewOrderDetail(orderId) {
    window.location.href = `/sales-staff/order-detail/${orderId}`;
}

function removeOrderFromList(orderId) {
    const ordersList = document.getElementById('pendingOrdersList');
    const orderItems = ordersList.querySelectorAll('.order-item');

    orderItems.forEach(item => {
        const orderIdElement = item.querySelector('.order-id');
        if (orderIdElement && orderIdElement.textContent === `#${orderId}`) {
            item.style.animation = 'fadeOut 0.3s ease';
            setTimeout(() => {
                item.remove();
                checkEmptyOrdersList();
            }, 300);
        }
    });
}

function checkEmptyOrdersList() {
    const ordersList = document.getElementById('pendingOrdersList');
    const orderItems = ordersList.querySelectorAll('.order-item');
    const noOrdersElement = document.querySelector('.no-orders');

    if (orderItems.length === 0) {
        noOrdersElement.style.display = 'block';
    }
}

// Quick actions
function createNewOrder() {
    window.location.href = '/sales-staff/create-order';
}

function registerCustomer() {
    window.location.href = '/sales-staff/register-customer';
}

function searchOrder() {
    const searchTerm = prompt('Nhập mã đơn hàng hoặc số điện thoại:');
    if (searchTerm && searchTerm.trim()) {
        window.location.href = `/sales-staff/search-order?q=${encodeURIComponent(searchTerm)}`;
    }
}

function viewShiftReport() {
    window.location.href = '/sales-staff/shift-report';
}

// Filter functions
function showOrderFilters() {
    // TODO: Show filter modal or dropdown
    showSalesNotification('Tính năng lọc đang được phát triển', 'info');
}

// Performance chart
function initializePerformanceChart() {
    const ctx = document.getElementById('performanceChart');
    if (ctx && typeof Chart !== 'undefined') {
        new Chart(ctx, {
            type: 'line',
            data: {
                labels: ['8h', '9h', '10h', '11h', '12h', '13h', '14h', '15h', '16h', '17h'],
                datasets: [{
                    label: 'Mục tiêu',
                    data: [3, 6, 9, 12, 15, 18, 21, 24, 27, 30],
                    borderColor: '#e74c3c',
                    backgroundColor: 'rgba(231, 76, 60, 0.1)',
                    borderDash: [5, 5],
                    tension: 0.4
                }, {
                    label: 'Thực tế',
                    data: [2, 5, 8, 11, 14, 16, 18, 20, 22, 23],
                    borderColor: '#2ecc71',
                    backgroundColor: 'rgba(46, 204, 113, 0.1)',
                    tension: 0.4,
                    fill: true
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true,
                        max: 35,
                        grid: {
                            color: '#f8f9fa'
                        }
                    },
                    x: {
                        grid: {
                            display: false
                        }
                    }
                }
            }
        });
    }
}

// Notification function
function showSalesNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `sales-notification ${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <i class="fas fa-${getNotificationIcon(type)}"></i>
            <span>${message}</span>
        </div>
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.classList.add('show');
    }, 100);

    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => {
            if (document.body.contains(notification)) {
                document.body.removeChild(notification);
            }
        }, 300);
    }, 3000);
}

function getNotificationIcon(type) {
    const icons = {
        'success': 'check-circle',
        'error': 'times-circle',
        'warning': 'exclamation-triangle',
        'info': 'info-circle'
    };
    return icons[type] || 'info-circle';
}

// CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes spin {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
    }
    
    @keyframes fadeOut {
        from { opacity: 1; transform: translateX(0); }
        to { opacity: 0; transform: translateX(20px); }
    }
    
    .sales-notification {
        position: fixed;
        top: 20px;
        right: 20px;
        background: white;
        padding: 16px 20px;
        border-radius: 8px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        z-index: 10000;
        transform: translateX(100%);
        transition: transform 0.3s ease;
        border-left: 4px solid #3498db;
        max-width: 300px;
    }
    
    .sales-notification.show {
        transform: translateX(0);
    }
    
    .sales-notification.success {
        border-left-color: #2ecc71;
    }
    
    .sales-notification.error {
        border-left-color: #e74c3c;
    }
    
    .sales-notification.warning {
        border-left-color: #f39c12;
    }
    
    .notification-content {
        display: flex;
        align-items: center;
        gap: 12px;
        color: #2c3e50;
        font-weight: 500;
    }
    
    .notification-content i {
        font-size: 1.1rem;
    }
    
    .sales-notification.success .notification-content i {
        color: #2ecc71;
    }
    
    .sales-notification.error .notification-content i {
        color: #e74c3c;
    }
    
    .sales-notification.warning .notification-content i {
        color: #f39c12;
    }
    
    .sales-notification.info .notification-content i {
        color: #3498db;
    }
`;
document.head.appendChild(style);
