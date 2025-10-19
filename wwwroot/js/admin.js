// Dashboard Functions
function refreshDashboard() {
    showNotification('Đang làm mới dữ liệu...', 'info');

    // Simulate data refresh
    setTimeout(() => {
        showNotification('Đã cập nhật dữ liệu thành công!', 'success');
        // Update charts and stats here
        updateDashboardCharts();
    }, 1500);
}

function updateDateRange() {
    const dateRange = document.getElementById('dateRange').value;
    showNotification(`Đã chuyển sang dữ liệu ${getDateRangeText(dateRange)}`, 'info');

    // Update dashboard data based on date range
    updateDashboardData(dateRange);
}

function getDateRangeText(range) {
    const ranges = {
        'today': 'hôm nay',
        'week': '7 ngày qua',
        'month': '30 ngày qua',
        'quarter': '90 ngày qua'
    };
    return ranges[range] || 'tùy chỉnh';
}

function updateDashboardData(range) {
    // Simulate updating dashboard stats
    const stats = generateMockStats(range);
    updateStatsCards(stats);
    updateActivityFeed();
    updateTopCustomers();
}

function generateMockStats(range) {
    // Mock data generation based on range
    const baseStats = {
        users: 1247,
        orders: 156,
        revenue: 18.5,
        satisfaction: 4.8
    };

    const multipliers = {
        'today': 0.1,
        'week': 0.5,
        'month': 1,
        'quarter': 3
    };

    const multiplier = multipliers[range] || 1;

    return {
        users: Math.floor(baseStats.users * multiplier),
        orders: Math.floor(baseStats.orders * multiplier),
        revenue: (baseStats.revenue * multiplier).toFixed(1),
        satisfaction: baseStats.satisfaction
    };
}

function updateStatsCards(stats) {
    document.querySelector('.users-card .stat-number').textContent = stats.users.toLocaleString();
    document.querySelector('.orders-card .stat-number').textContent = stats.orders;
    document.querySelector('.revenue-card .stat-number').textContent = stats.revenue + 'M';
    document.querySelector('.satisfaction-card .stat-number').textContent = stats.satisfaction;
}

// Period selector for quick stats
document.addEventListener('DOMContentLoaded', function () {
    const periodBtns = document.querySelectorAll('.period-btn');
    periodBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            periodBtns.forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            const period = this.dataset.period;
            updateQuickStats(period);
        });
    });

    // Chart controls
    const chartBtns = document.querySelectorAll('.chart-btn');
    chartBtns.forEach(btn => {
        btn.addEventListener('click', function () {
            chartBtns.forEach(b => b.classList.remove('active'));
            this.classList.add('active');

            const chartType = this.dataset.chart;
            updateMainChart(chartType);
        });
    });
});

function updateQuickStats(period) {
    const mockData = {
        today: { users: 23, orders: 156, reviews: 89, support: 12 },
        week: { users: 167, orders: 892, reviews: 456, support: 78 },
        month: { users: 672, orders: 3421, reviews: 1890, support: 234 }
    };

    const data = mockData[period] || mockData.today;

    document.querySelector('.quick-stat-item:nth-child(1) .stat-value').textContent = data.users;
    document.querySelector('.quick-stat-item:nth-child(2) .stat-value').textContent = data.orders;
    document.querySelector('.quick-stat-item:nth-child(3) .stat-value').textContent = data.reviews;
    document.querySelector('.quick-stat-item:nth-child(4) .stat-value').textContent = data.support;
}

// User Management Functions
function filterUsers() {
    const searchTerm = document.getElementById('searchInput').value.toLowerCase();
    const statusFilter = document.getElementById('statusFilter').value;
    const roleFilter = document.getElementById('roleFilter').value;
    const dateFrom = document.getElementById('dateFromFilter').value;
    const dateTo = document.getElementById('dateToFilter').value;

    const rows = document.querySelectorAll('#usersTableBody tr');
    let visibleCount = 0;

    rows.forEach(row => {
        let visible = true;

        // Search filter
        if (searchTerm) {
            const userName = row.querySelector('.user-name').textContent.toLowerCase();
            const userEmail = row.querySelector('.email').textContent.toLowerCase();
            const userPhone = row.querySelector('.phone').textContent.toLowerCase();

            if (!userName.includes(searchTerm) &&
                !userEmail.includes(searchTerm) &&
                !userPhone.includes(searchTerm)) {
                visible = false;
            }
        }

        // Status filter
        if (statusFilter && visible) {
            const status = row.querySelector('.status-badge').textContent.toLowerCase();
            if (!status.includes(statusFilter)) {
                visible = false;
            }
        }

        // Role filter
        if (roleFilter && visible) {
            const role = row.querySelector('.role-badge').textContent.toLowerCase();
            if (!role.includes(roleFilter)) {
                visible = false;
            }
        }

        // Date filters would be implemented here

        row.style.display = visible ? '' : 'none';
        if (visible) visibleCount++;
    });

    // Update counters
    const totalCount = document.querySelector('.total-count');
    const filteredCount = document.querySelector('.filtered-count');

    if (searchTerm || statusFilter || roleFilter || dateFrom || dateTo) {
        totalCount.style.display = 'none';
        filteredCount.style.display = 'block';
        filteredCount.innerHTML = `Tìm thấy: <strong>${visibleCount}</strong> kết quả`;
    } else {
        totalCount.style.display = 'block';
        filteredCount.style.display = 'none';
    }
}

function resetFilters() {
    document.getElementById('searchInput').value = '';
    document.getElementById('statusFilter').value = '';
    document.getElementById('roleFilter').value = '';
    document.getElementById('dateFromFilter').value = '';
    document.getElementById('dateToFilter').value = '';

    filterUsers();
    showNotification('Đã reset bộ lọc', 'info');
}

function toggleSelectAll() {
    const selectAll = document.getElementById('selectAll');
    const checkboxes = document.querySelectorAll('.user-checkbox');

    checkboxes.forEach(checkbox => {
        checkbox.checked = selectAll.checked;
    });

    updateBulkActions();
}

function updateBulkActions() {
    const checkedBoxes = document.querySelectorAll('.user-checkbox:checked');
    const bulkButtons = document.querySelectorAll('.btn-bulk-action');

    const hasSelection = checkedBoxes.length > 0;
    bulkButtons.forEach(btn => {
        btn.disabled = !hasSelection;
    });
}

// User action functions
function viewUser(userId) {
    // Show user details modal
    const modal = document.getElementById('userDetailsModal');
    loadUserDetails(userId);
    modal.classList.add('active');
}

function editUser(userId) {
    showNotification(`Chỉnh sửa user ID: ${userId}`, 'info');
    // Implement edit functionality
}

function deleteUser(userId) {
    if (confirm('Bạn có chắc muốn xóa người dùng này?')) {
        showNotification(`Đã xóa user ID: ${userId}`, 'success');
        // Implement delete functionality
    }
}

function banUser(userId) {
    if (confirm('Bạn có chắc muốn cấm tài khoản này?')) {
        showNotification(`Đã cấm user ID: ${userId}`, 'warning');
        // Implement ban functionality
    }
}

function resetPassword(userId) {
    if (confirm('Bạn có chắc muốn reset mật khẩu cho user này?')) {
        showNotification(`Đã reset mật khẩu cho user ID: ${userId}`, 'success');
        // Implement password reset
    }
}

function sendEmail(userId) {
    showNotification(`Mở form gửi email cho user ID: ${userId}`, 'info');
    // Implement email functionality
}

function viewOrders(userId) {
    showNotification(`Xem đơn hàng của user ID: ${userId}`, 'info');
    // Redirect to orders page with user filter
}

function toggleDropdown(userId) {
    const dropdown = document.getElementById(`dropdown-${userId}`);

    // Close all other dropdowns
    document.querySelectorAll('.dropdown-menu').forEach(menu => {
        if (menu.id !== `dropdown-${userId}`) {
            menu.classList.remove('show');
        }
    });

    dropdown.classList.toggle('show');
}

function loadUserDetails(userId) {
    // Mock user data loading
    const userData = {
        1: {
            name: 'Nguyễn Văn A',
            email: 'nguyenvana@email.com',
            avatar: '/Images/users/user1.jpg',
            role: 'VIP',
            status: 'Hoạt động',
            totalOrders: 47,
            totalSpent: '12.5M',
            avgRating: 4.8
        }
        // Add more mock data as needed
    };

    const user = userData[userId] || userData[1];

    document.getElementById('modalUserName').textContent = user.name;
    document.getElementById('modalUserEmail').textContent = user.email;
    document.getElementById('modalUserAvatar').src = user.avatar;
    document.getElementById('modalUserRole').textContent = user.role;
    document.getElementById('modalUserStatus').textContent = user.status;
    document.getElementById('modalTotalOrders').textContent = user.totalOrders;
    document.getElementById('modalTotalSpent').textContent = user.totalSpent;
    document.getElementById('modalAvgRating').textContent = user.avgRating;
}

function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('active');
}

function showTab(tabId) {
    // Hide all tab contents
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.remove('active');
    });

    // Remove active class from all tab buttons
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });

    // Show selected tab
    document.getElementById(tabId).classList.add('active');

    // Add active class to clicked button
    event.target.classList.add('active');

    // Load tab content
    loadTabContent(tabId);
}

function loadTabContent(tabId) {
    const content = document.getElementById(tabId);

    // Mock content loading
    switch (tabId) {
        case 'orders-tab':
            content.innerHTML = '<p>Đang tải danh sách đơn hàng...</p>';
            break;
        case 'reviews-tab':
            content.innerHTML = '<p>Đang tải danh sách đánh giá...</p>';
            break;
        case 'activity-tab':
            content.innerHTML = '<p>Đang tải lịch sử hoạt động...</p>';
            break;
    }
}

// Search functionality
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('searchInput');
    if (searchInput) {
        searchInput.addEventListener('input', filterUsers);
    }

    // Add event listeners for checkboxes
    document.querySelectorAll('.user-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', updateBulkActions);
    });

    // Close dropdowns when clicking outside
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.dropdown')) {
            document.querySelectorAll('.dropdown-menu').forEach(menu => {
                menu.classList.remove('show');
            });
        }
    });
});

// Export functions
function exportUsers() {
    showNotification('Đang xuất file Excel...', 'info');

    // Simulate export
    setTimeout(() => {
        showNotification('Đã xuất file thành công!', 'success');
    }, 2000);
}

function bulkDelete() {
    const checkedBoxes = document.querySelectorAll('.user-checkbox:checked');
    if (confirm(`Bạn có chắc muốn xóa ${checkedBoxes.length} người dùng đã chọn?`)) {
        showNotification(`Đã xóa ${checkedBoxes.length} người dùng`, 'success');
        // Implement bulk delete
    }
}

function bulkStatus() {
    const checkedBoxes = document.querySelectorAll('.user-checkbox:checked');
    showNotification(`Thay đổi trạng thái cho ${checkedBoxes.length} người dùng`, 'info');
    // Implement bulk status change
}

// Charts initialization
function updateDashboardCharts() {
    // Initialize dashboard charts if Chart.js is available
    if (typeof Chart !== 'undefined') {
        initializeDashboardCharts();
    }
}

function initializeDashboardCharts() {
    // Mini charts for stat cards
    const chartOptions = {
        responsive: true,
        maintainAspectRatio: false,
        plugins: { legend: { display: false } },
        scales: {
            x: { display: false },
            y: { display: false }
        },
        elements: { point: { radius: 0 } }
    };

    // Users chart
    const usersCtx = document.getElementById('usersChart');
    if (usersCtx) {
        new Chart(usersCtx, {
            type: 'line',
            data: {
                labels: ['', '', '', '', '', '', ''],
                datasets: [{
                    data: [10, 15, 12, 20, 18, 25, 23],
                    borderColor: '#3498db',
                    backgroundColor: 'rgba(52, 152, 219, 0.1)',
                    fill: true,
                    tension: 0.4
                }]
            },
            options: chartOptions
        });
    }

    // Main dashboard chart
    updateMainChart('orders');

    // Region chart
    const regionCtx = document.getElementById('regionChart');
    if (regionCtx) {
        new Chart(regionCtx, {
            type: 'doughnut',
            data: {
                labels: ['Quận 1', 'Quận 3', 'Quận 7', 'Khác'],
                datasets: [{
                    data: [35, 25, 20, 20],
                    backgroundColor: ['#e74c3c', '#3498db', '#2ecc71', '#95a5a6']
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                plugins: {
                    legend: { position: 'bottom' }
                }
            }
        });
    }
}

function updateMainChart(type) {
    const ctx = document.getElementById('mainChart');
    if (!ctx) return;

    // Destroy existing chart
    if (window.mainChartInstance) {
        window.mainChartInstance.destroy();
    }

    const chartData = {
        orders: {
            labels: ['1/12', '5/12', '10/12', '15/12', '20/12', '25/12', '30/12'],
            data: [120, 150, 130, 180, 165, 200, 190],
            color: '#e74c3c'
        },
        revenue: {
            labels: ['1/12', '5/12', '10/12', '15/12', '20/12', '25/12', '30/12'],
            data: [12, 15, 13, 18, 16.5, 20, 19],
            color: '#2ecc71'
        },
        users: {
            labels: ['1/12', '5/12', '10/12', '15/12', '20/12', '25/12', '30/12'],
            data: [1200, 1220, 1210, 1235, 1240, 1250, 1247],
            color: '#3498db'
        }
    };

    const data = chartData[type];

    window.mainChartInstance = new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.labels,
            datasets: [{
                label: type,
                data: data.data,
                borderColor: data.color,
                backgroundColor: data.color + '20',
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: {
                legend: { display: false }
            },
            scales: {
                y: { beginAtZero: true }
            }
        }
    });
}

// Orders Management Functions
function filterOrders() {
    const status = document.getElementById('statusFilter').value;
    const date = document.getElementById('dateFilter').value;
    const search = document.getElementById('searchFilter').value.toLowerCase();

    const rows = document.querySelectorAll('#ordersTableBody tr');

    rows.forEach(row => {
        const rowStatus = row.dataset.status;
        const rowDate = row.dataset.date;
        const rowText = row.textContent.toLowerCase();

        const statusMatch = !status || rowStatus === status;
        const dateMatch = !date || rowDate === date;
        const searchMatch = !search || rowText.includes(search);

        if (statusMatch && dateMatch && searchMatch) {
            row.style.display = '';
        } else {
            row.style.display = 'none';
        }
    });
}

function clearFilters() {
    document.getElementById('statusFilter').value = '';
    document.getElementById('dateFilter').value = '';
    document.getElementById('searchFilter').value = '';
    filterOrders();
}

function viewOrder(orderId) {
    document.getElementById('orderDetailModal').classList.add('active');
}

function confirmOrder(orderId) {
    if (confirm('Xác nhận đơn hàng này?')) {
        showAdminNotification('Đã xác nhận đơn hàng #DH' + orderId.toString().padStart(3, '0'), 'success');
    }
}

function cancelOrder(orderId) {
    if (confirm('Hủy đơn hàng này?')) {
        showAdminNotification('Đã hủy đơn hàng #DH' + orderId.toString().padStart(3, '0'), 'warning');
    }
}

function exportOrders() {
    showAdminNotification('Đang xuất file Excel...', 'info');
    // Export logic here
}

function printOrder() {
    window.print();
}

// Reviews Management Functions
function filterReviews() {
    const rating = document.getElementById('ratingFilter').value;
    const status = document.getElementById('statusFilter').value;
    const date = document.getElementById('dateFilter').value;
    const search = document.getElementById('searchFilter').value.toLowerCase();

    const reviews = document.querySelectorAll('.review-item');

    reviews.forEach(review => {
        const reviewRating = review.dataset.rating;
        const reviewStatus = review.dataset.status;
        const reviewText = review.textContent.toLowerCase();

        const ratingMatch = !rating || reviewRating === rating;
        const statusMatch = !status || reviewStatus === status;
        const searchMatch = !search || reviewText.includes(search);

        if (ratingMatch && statusMatch && searchMatch) {
            review.style.display = '';
        } else {
            review.style.display = 'none';
        }
    });
}

function approveReview(reviewId) {
    showAdminNotification('Đã duyệt đánh giá', 'success');
}

function rejectReview(reviewId) {
    if (confirm('Từ chối đánh giá này?')) {
        showAdminNotification('Đã từ chối đánh giá', 'warning');
    }
}

function replyReview(reviewId) {
    const replySection = document.querySelector(`.review-item:nth-child(${reviewId}) .admin-reply`);
    replySection.style.display = 'block';
}

function sendReply(reviewId) {
    showAdminNotification('Đã gửi phản hồi', 'success');
    const replySection = document.querySelector(`.review-item:nth-child(${reviewId}) .admin-reply`);
    replySection.style.display = 'none';
}

function deleteReview(reviewId) {
    if (confirm('Xóa đánh giá này?')) {
        showAdminNotification('Đã xóa đánh giá', 'success');
    }
}

// Reports Functions
function updateReportPeriod() {
    const period = document.getElementById('reportPeriod').value;
    showAdminNotification('Đang cập nhật báo cáo...', 'info');
    // Update charts and data
}

function showReportTab(tabName) {
    // Hide all tabs
    document.querySelectorAll('.tab-content').forEach(tab => {
        tab.classList.remove('active');
    });

    // Remove active class from all buttons
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });

    // Show selected tab
    document.getElementById(tabName).classList.add('active');
    event.target.classList.add('active');
}

function switchChart(chartType, period) {
    const buttons = event.target.parentElement.querySelectorAll('.chart-btn');
    buttons.forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');

    showAdminNotification(`Đang tải dữ liệu ${period}...`, 'info');
}

function exportReport() {
    showAdminNotification('Đang xuất báo cáo...', 'info');
}

// Modal functions
function closeModal(modalId) {
    document.getElementById(modalId).classList.remove('active');
}

// Pagination
function changePage(direction) {
    showAdminNotification('Đang tải trang...', 'info');
}

// Initialize charts (placeholder - would use Chart.js)
document.addEventListener('DOMContentLoaded', function () {
    // Initialize charts here with Chart.js
    if (typeof Chart !== 'undefined') {
        // Revenue Chart
        const revenueCtx = document.getElementById('revenueChart');
        if (revenueCtx) {
            new Chart(revenueCtx, {
                type: 'line',
                data: {
                    labels: ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'],
                    datasets: [{
                        label: 'Doanh thu',
                        data: [3.2, 4.1, 3.8, 5.2, 4.7, 3.9, 4.5],
                        borderColor: '#28a745',
                        backgroundColor: 'rgba(40, 167, 69, 0.1)',
                        tension: 0.4
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false
                }
            });
        }

        // Order Status Chart
        const statusCtx = document.getElementById('orderStatusChart');
        if (statusCtx) {
            new Chart(statusCtx, {
                type: 'doughnut',
                data: {
                    labels: ['Hoàn thành', 'Đang giao', 'Đã xác nhận', 'Chờ xác nhận'],
                    datasets: [{
                        data: [234, 45, 89, 24],
                        backgroundColor: [
                            '#28a745',
                            '#fd7e14',
                            '#17a2b8',
                            '#ffc107'
                        ]
                    }]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false
                }
            });
        }
    }
});

