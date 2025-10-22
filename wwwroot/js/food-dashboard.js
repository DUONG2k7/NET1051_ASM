// Food Dashboard functionality
document.addEventListener('DOMContentLoaded', function () {
    initializeFoodCharts();
    setupFoodEventListeners();
    loadFoodData();
});

// Chart initialization
function initializeFoodCharts() {
    initCategoryRevenueChart();
    initCategoryDistributionChart();
}

function initCategoryRevenueChart() {
    const ctx = document.getElementById('categoryRevenueChart');
    if (ctx) {
        new Chart(ctx, {
            type: 'bar',
            data: {
                labels: ['Cơm', 'Phở', 'Bánh mì', 'Bún', 'Thức uống', 'Tráng miệng'],
                datasets: [{
                    label: 'Doanh thu (triệu VNĐ)',
                    data: [12.5, 8.9, 6.2, 5.8, 4.3, 2.1],
                    backgroundColor: [
                        '#2ecc71',
                        '#3498db',
                        '#f39c12',
                        '#e74c3c',
                        '#9b59b6',
                        '#1abc9c'
                    ]
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

function initCategoryDistributionChart() {
    const ctx = document.getElementById('categoryDistributionChart');
    if (ctx) {
        new Chart(ctx, {
            type: 'doughnut',
            data: {
                labels: ['Cơm', 'Phở', 'Bánh mì', 'Bún', 'Thức uống', 'Khác'],
                datasets: [{
                    data: [35, 25, 15, 12, 8, 5],
                    backgroundColor: [
                        '#2ecc71',
                        '#3498db',
                        '#f39c12',
                        '#e74c3c',
                        '#9b59b6',
                        '#95a5a6'
                    ]
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }
}

// Event listeners
function setupFoodEventListeners() {
    // Date range selector
    const dateRange = document.getElementById('foodDateRange');
    if (dateRange) {
        dateRange.addEventListener('change', updateFoodDateRange);
    }

    // Bestseller period selector
    const bestsellerPeriod = document.getElementById('bestsellerPeriod');
    if (bestsellerPeriod) {
        bestsellerPeriod.addEventListener('change', updateBestsellers);
    }
}

// Dashboard functions
function updateFoodDateRange() {
    const range = document.getElementById('foodDateRange').value;
    showNotification(`Đã cập nhật dữ liệu cho ${getFoodDateRangeText(range)}`, 'info');
    updateFoodStats(range);
}

function getFoodDateRangeText(range) {
    const texts = {
        'today': 'hôm nay',
        'week': '7 ngày qua',
        'month': '30 ngày qua',
        'quarter': '3 tháng qua'
    };
    return texts[range] || 'thời gian đã chọn';
}

function refreshFoodDashboard() {
    const refreshBtn = document.querySelector('.btn-refresh');
    const icon = refreshBtn.querySelector('i');

    // Add spinning animation
    icon.style.animation = 'spin 1s linear infinite';
    refreshBtn.disabled = true;

    // Simulate data refresh
    setTimeout(() => {
        icon.style.animation = '';
        refreshBtn.disabled = false;
        showNotification('Đã làm mới dữ liệu dashboard', 'success');
        loadFoodData();
    }, 1500);
}

function loadFoodData() {
    // TODO: Load actual data from API
    console.log('Loading food dashboard data...');
}

function updateFoodStats(range) {
    // TODO: Update stats based on date range
    console.log('Updating food stats for range:', range);
}

function updateBestsellers() {
    const period = document.getElementById('bestsellerPeriod').value;
    showNotification(`Đang tải top món bán chạy ${period}...`, 'info');
    // TODO: Update bestsellers list
}

// Quick action functions
function addNewItem() {
    window.location.href = '/food-admin/menu-management?action=add';
}

function manageCategories() {
    window.location.href = '/food-admin/category-management';
}

function createDiscount() {
    window.location.href = '/food-admin/discount-management?action=create';
}

function viewReports() {
    window.location.href = '/food-admin/sales-report';
}

// Alert functions
function checkExpiredDiscounts() {
    window.location.href = '/food-admin/discount-management?filter=expiring';
}

function checkLowStock() {
    window.location.href = '/food-admin/menu-management?filter=low_stock';
}

function viewNewItems() {
    window.location.href = '/food-admin/menu-management?filter=recent';
}

// Chart switching
function switchRevenueChart(period) {
    const buttons = event.target.parentElement.querySelectorAll('.chart-btn');
    buttons.forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');

    showNotification(`Đang tải dữ liệu doanh thu ${period}...`, 'info');
    // TODO: Update chart data
}

// Utility functions
function showNotification(message, type = 'info') {
    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <i class="fas fa-${getNotificationIcon(type)}"></i>
            <span>${message}</span>
        </div>
    `;

    // Add to page
    document.body.appendChild(notification);

    // Show notification
    setTimeout(() => {
        notification.classList.add('show');
    }, 100);

    // Hide notification after 3 seconds
    setTimeout(() => {
        notification.classList.remove('show');
        setTimeout(() => {
            document.body.removeChild(notification);
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

// CSS for notifications and animations
const style = document.createElement('style');
style.textContent = `
    @keyframes spin {
        from { transform: rotate(0deg); }
        to { transform: rotate(360deg); }
    }
    
    .notification {
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
    }
    
    .notification.show {
        transform: translateX(0);
    }
    
    .notification.success {
        border-left-color: #2ecc71;
    }
    
    .notification.error {
        border-left-color: #e74c3c;
    }
    
    .notification.warning {
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
    
    .notification.success .notification-content i {
        color: #2ecc71;
    }
    
    .notification.error .notification-content i {
        color: #e74c3c;
    }
    
    .notification.warning .notification-content i {
        color: #f39c12;
    }
    
    .notification.info .notification-content i {
        color: #3498db;
    }
`;
document.head.appendChild(style);
