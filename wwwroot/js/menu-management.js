// menu-management.js

// Global variables
let currentEditingId = null;
let currentView = 'grid';

// DOM loaded event
document.addEventListener('DOMContentLoaded', function () {
    initializeMenuManagement();
});

// Initialize menu management
function initializeMenuManagement() {
    // Add search functionality
    const searchInput = document.getElementById('menuSearchInput');
    if (searchInput) {
        searchInput.addEventListener('input', debounce(filterMenu, 300));
    }

    // Add checkbox event listeners
    document.querySelectorAll('.menu-checkbox').forEach(checkbox => {
        checkbox.addEventListener('change', updateBulkActions);
    });

    // Close modal when clicking outside
    document.addEventListener('click', function (e) {
        if (e.target.classList.contains('modal')) {
            closeMenuItemModal();
        }
    });

    // Initialize view
    switchView('grid');
}

// Debounce function for search
function debounce(func, wait) {
    let timeout;
    return function executedFunction(...args) {
        const later = () => {
            clearTimeout(timeout);
            func(...args);
        };
        clearTimeout(timeout);
        timeout = setTimeout(later, wait);
    };
}

// Modal Functions
function showAddItemModal() {
    currentEditingId = null;
    document.getElementById('menuItemModalTitle').textContent = 'Thêm món mới';
    resetMenuForm();
    document.getElementById('menuItemModal').style.display = 'flex';
}

function closeMenuItemModal() {
    document.getElementById('menuItemModal').style.display = 'none';
    resetMenuForm();
}

function resetMenuForm() {
    document.getElementById('menuItemForm').reset();
    removeImage();
    currentEditingId = null;
}

// Image Upload Functions
function triggerImageUpload() {
    document.getElementById('itemImage').click();
}

function previewImage() {
    const file = document.getElementById('itemImage').files[0];
    if (file) {
        //// Validate file type
        //if (!file.type.startsWith('image/')) {
        //    showNotification('Vui lòng chọn file hình ảnh hợp lệ', 'error');
        //    return;
        //}

        //// Validate file size (max 5MB)
        //if (file.size > 5 * 1024 * 1024) {
        //    showNotification('Kích thước file không được vượt quá 5MB', 'error');
        //    return;
        //}

        const reader = new FileReader();
        reader.onload = function (e) {
            document.getElementById('previewImg').src = e.target.result;
            document.getElementById('imagePreview').style.display = 'block';
            document.querySelector('.upload-box').style.display = 'none';
        };
        reader.readAsDataURL(file);
    }
}

function removeImage() {
    document.getElementById('itemImage').value = '';
    document.getElementById('imagePreview').style.display = 'none';
    document.querySelector('.upload-box').style.display = 'block';
}

// Save Menu Item
function saveMenuItem() {
    const form = document.getElementById('menuItemForm');
    const formData = new FormData(form);

    // Validate required fields
    const itemName = document.getElementById('itemName').value.trim();
    const itemCategory = document.getElementById('itemCategory').value;
    const itemPrice = document.getElementById('itemPrice').value;

    if (!itemName || !itemCategory || !itemPrice) {
        showNotification('Vui lòng điền đầy đủ thông tin bắt buộc', 'error');
        return;
    }

    if (parseFloat(itemPrice) <= 0) {
        showNotification('Giá món ăn phải lớn hơn 0', 'error');
        return;
    }

    // Show loading
    showNotification('Đang lưu món ăn...', 'info');

    // Simulate API call
    setTimeout(() => {
        if (currentEditingId) {
            showNotification('Đã cập nhật món ăn thành công!', 'success');
        } else {
            showNotification('Đã thêm món ăn mới thành công!', 'success');
        }

        closeMenuItemModal();
        // Refresh the menu list here
        // refreshMenuList();
    }, 1500);
}

// Edit Menu Item
function editMenuItem(itemId) {
    currentEditingId = itemId;
    document.getElementById('menuItemModalTitle').textContent = 'Chỉnh sửa món ăn';

    // Mock data loading
    loadMenuItemData(itemId);

    document.getElementById('menuItemModal').style.display = 'flex';
}

function loadMenuItemData(itemId) {
    // Mock data - replace with actual API call
    const mockData = {
        1: {
            itemName: 'Cơm tấm sườn nướng',
            itemCategory: 'rice',
            itemPrice: 45000,
            prepTime: 20,
            itemStatus: 'available',
            itemDescription: 'Cơm tấm thơm ngon với sườn nướng bbq, chả trứng, bì và nước mắm chua ngọt',
            isSpicy: false,
            isVegetarian: false,
            isSpecial: true,
            ingredients: 'Sườn heo, cơm tấm, chả trứng, bì',
            allergens: 'Không có'
        },
        2: {
            itemName: 'Phở bò tái',
            itemCategory: 'noodle',
            itemPrice: 65000,
            prepTime: 15,
            itemStatus: 'available',
            itemDescription: 'Phở bò tái truyền thống với nước dùng trong, thịt bò tái mềm',
            isSpicy: false,
            isVegetarian: false,
            isSpecial: false,
            ingredients: 'Bánh phở, thịt bò tái, hành lá, ngò gai',
            allergens: 'Gluten'
        }
    };

    const data = mockData[itemId] || mockData[1];

    // Populate form
    document.getElementById('itemName').value = data.itemName;
    document.getElementById('itemCategory').value = data.itemCategory;
    document.getElementById('itemPrice').value = data.itemPrice;
    document.getElementById('prepTime').value = data.prepTime;
    document.getElementById('itemStatus').value = data.itemStatus;
    document.getElementById('itemDescription').value = data.itemDescription;
    document.getElementById('isSpicy').checked = data.isSpicy;
    document.getElementById('isVegetarian').checked = data.isVegetarian;
    document.getElementById('isSpecial').checked = data.isSpecial;
    document.getElementById('ingredients').value = data.ingredients;
    document.getElementById('allergens').value = data.allergens;
}

// Copy Menu Item
function copyMenuItem(itemId) {
    if (confirm('Bạn có muốn sao chép món ăn này không?')) {
        showNotification('Đang sao chép món ăn...', 'info');

        // Load data and open modal for new item
        setTimeout(() => {
            currentEditingId = null;
            document.getElementById('menuItemModalTitle').textContent = 'Thêm món mới (Sao chép)';
            loadMenuItemData(itemId);

            // Clear the name and add "Copy" suffix
            const currentName = document.getElementById('itemName').value;
            document.getElementById('itemName').value = currentName + ' (Bản sao)';

            document.getElementById('menuItemModal').style.display = 'flex';
            showNotification('Đã tải dữ liệu để sao chép', 'success');
        }, 500);
    }
}

// Delete Menu Item
function deleteMenuItem(itemId) {
    if (confirm('Bạn có chắc muốn xóa món ăn này? Hành động này không thể hoàn tác.')) {
        showNotification('Đang xóa món ăn...', 'info');

        setTimeout(() => {
            showNotification('Đã xóa món ăn thành công!', 'success');
            // Remove item from DOM
            removeMenuItemFromDOM(itemId);
        }, 1000);
    }
}

function removeMenuItemFromDOM(itemId) {
    // Remove from grid view
    const gridItem = document.querySelector(`.menu-item-card .btn-action[onclick*="${itemId}"]`)?.closest('.menu-item-card');
    if (gridItem) {
        gridItem.remove();
    }

    // Remove from list view
    const listItem = document.querySelector(`.menu-checkbox[value="${itemId}"]`)?.closest('tr');
    if (listItem) {
        listItem.remove();
    }
}

// View Toggle Functions
function switchView(viewType) {
    currentView = viewType;

    // Update buttons
    document.querySelectorAll('.view-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    document.querySelector(`[data-view="${viewType}"]`).classList.add('active');

    // Show/hide views
    document.getElementById('gridView').classList.toggle('active', viewType === 'grid');
    document.getElementById('listView').style.display = viewType === 'list' ? 'block' : 'none';

    showNotification(`Đã chuyển sang chế độ xem ${viewType === 'grid' ? 'lưới' : 'danh sách'}`, 'info');
}

// Filter Functions
function filterMenu() {
    const searchTerm = document.getElementById('menuSearchInput').value.toLowerCase();
    const categoryFilter = document.getElementById('categoryFilter').value;
    const statusFilter = document.getElementById('statusFilter').value;
    const priceFilter = document.getElementById('priceFilter').value;

    if (currentView === 'grid') {
        filterGridView(searchTerm, categoryFilter, statusFilter, priceFilter);
    } else {
        filterListView(searchTerm, categoryFilter, statusFilter, priceFilter);
    }
}

function filterGridView(searchTerm, categoryFilter, statusFilter, priceFilter) {
    const items = document.querySelectorAll('.menu-item-card');
    let visibleCount = 0;

    items.forEach(item => {
        const itemName = item.querySelector('.item-name').textContent.toLowerCase();
        const itemCategory = item.dataset.category;
        const itemStatus = item.dataset.status;
        const itemPrice = parseFloat(item.querySelector('.item-price').textContent.replace(/[^\d]/g, ''));

        let visible = true;

        // Search filter
        if (searchTerm && !itemName.includes(searchTerm)) {
            visible = false;
        }

        // Category filter
        if (categoryFilter && itemCategory !== categoryFilter) {
            visible = false;
        }

        // Status filter
        if (statusFilter && itemStatus !== statusFilter) {
            visible = false;
        }

        // Price filter
        if (priceFilter && visible) {
            switch (priceFilter) {
                case 'under_50k':
                    if (itemPrice >= 50000) visible = false;
                    break;
                case '50k_100k':
                    if (itemPrice < 50000 || itemPrice > 100000) visible = false;
                    break;
                case 'over_100k':
                    if (itemPrice <= 100000) visible = false;
                    break;
            }
        }

        item.style.display = visible ? 'block' : 'none';
        if (visible) visibleCount++;
    });

    updateFilterResults(visibleCount);
}

function filterListView(searchTerm, categoryFilter, statusFilter, priceFilter) {
    const rows = document.querySelectorAll('.menu-table tbody tr');
    let visibleCount = 0;

    rows.forEach(row => {
        const itemName = row.querySelector('.item-name').textContent.toLowerCase();
        const categoryTag = row.querySelector('.category-tag');
        const statusBadge = row.querySelector('.status-badge');
        const priceText = row.querySelector('.price').textContent;
        const itemPrice = parseFloat(priceText.replace(/[^\d]/g, ''));

        let visible = true;

        // Search filter
        if (searchTerm && !itemName.includes(searchTerm)) {
            visible = false;
        }

        // Category filter
        if (categoryFilter && !categoryTag.classList.contains(categoryFilter)) {
            visible = false;
        }

        // Status filter
        if (statusFilter && !statusBadge.classList.contains(statusFilter)) {
            visible = false;
        }

        // Price filter
        if (priceFilter && visible) {
            switch (priceFilter) {
                case 'under_50k':
                    if (itemPrice >= 50000) visible = false;
                    break;
                case '50k_100k':
                    if (itemPrice < 50000 || itemPrice > 100000) visible = false;
                    break;
                case 'over_100k':
                    if (itemPrice <= 100000) visible = false;
                    break;
            }
        }

        row.style.display = visible ? '' : 'none';
        if (visible) visibleCount++;
    });

    updateFilterResults(visibleCount);
}

function resetMenuFilters() {
    document.getElementById('menuSearchInput').value = '';
    document.getElementById('categoryFilter').value = '';
    document.getElementById('statusFilter').value = '';
    document.getElementById('priceFilter').value = '';

    filterMenu();
    showNotification('Đã xóa tất cả bộ lọc', 'info');
}

function updateFilterResults(visibleCount) {
    const paginationInfo = document.querySelector('.pagination-info');
    if (paginationInfo) {
        paginationInfo.textContent = `Hiển thị ${visibleCount} món ăn`;
    }
}

// Checkbox Functions
function toggleSelectAllMenu() {
    const selectAll = document.getElementById('selectAllMenu');
    const checkboxes = document.querySelectorAll('.menu-checkbox');

    checkboxes.forEach(checkbox => {
        checkbox.checked = selectAll.checked;
    });

    updateBulkActions();
}

function updateBulkActions() {
    const checkedBoxes = document.querySelectorAll('.menu-checkbox:checked');
    const bulkButtons = document.querySelectorAll('.btn-bulk-action');
    const hasSelection = checkedBoxes.length > 0;

    bulkButtons.forEach(btn => {
        btn.disabled = !hasSelection;
    });

    // Update select all checkbox state
    const selectAll = document.getElementById('selectAllMenu');
    const allCheckboxes = document.querySelectorAll('.menu-checkbox');
    if (selectAll) {
        selectAll.indeterminate = checkedBoxes.length > 0 && checkedBoxes.length < allCheckboxes.length;
        selectAll.checked = checkedBoxes.length === allCheckboxes.length && allCheckboxes.length > 0;
    }
}

// Import/Export Functions
function importMenu() {
    // Create file input dynamically
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.xlsx,.xls';
    input.onchange = function (e) {
        const file = e.target.files[0];
        if (file) {
            showNotification('Đang import dữ liệu từ Excel...', 'info');

            // Simulate import process
            setTimeout(() => {
                showNotification('Import thành công 25 món ăn mới!', 'success');
                // Refresh menu list here
            }, 3000);
        }
    };
    input.click();
}

function exportMenu() {
    showNotification('Đang xuất dữ liệu ra Excel...', 'info');

    // Simulate export process
    setTimeout(() => {
        showNotification('Đã xuất file Excel thành công!', 'success');

        // Create download link (mock)
        const link = document.createElement('a');
        link.href = '#';
        link.download = 'danh-sach-menu.xlsx';
        // link.click(); // Uncomment when implementing real export
    }, 2000);
}

// Bulk Actions
function bulkDeleteItems() {
    const checkedBoxes = document.querySelectorAll('.menu-checkbox:checked');
    if (checkedBoxes.length === 0) {
        showNotification('Vui lòng chọn món ăn cần xóa', 'warning');
        return;
    }

    if (confirm(`Bạn có chắc muốn xóa ${checkedBoxes.length} món ăn đã chọn?`)) {
        showNotification('Đang xóa các món ăn...', 'info');

        setTimeout(() => {
            checkedBoxes.forEach(checkbox => {
                const itemId = checkbox.value;
                removeMenuItemFromDOM(itemId);
            });

            showNotification(`Đã xóa ${checkedBoxes.length} món ăn thành công!`, 'success');
            updateBulkActions();
        }, 1500);
    }
}

function bulkUpdateStatus(newStatus) {
    const checkedBoxes = document.querySelectorAll('.menu-checkbox:checked');
    if (checkedBoxes.length === 0) {
        showNotification('Vui lòng chọn món ăn cần cập nhật', 'warning');
        return;
    }

    const statusText = {
        'available': 'Đang bán',
        'unavailable': 'Tạm dừng',
        'out_of_stock': 'Hết hàng'
    };

    showNotification(`Đang cập nhật trạng thái thành "${statusText[newStatus]}"...`, 'info');

    setTimeout(() => {
        showNotification(`Đã cập nhật trạng thái cho ${checkedBoxes.length} món ăn!`, 'success');
    }, 1000);
}

// Notification Function
function showNotification(message, type = 'info') {
    // Remove existing notifications
    const existingNotifications = document.querySelectorAll('.notification');
    existingNotifications.forEach(notification => notification.remove());

    // Create notification element
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.innerHTML = `
        <div class="notification-content">
            <i class="fas fa-${getNotificationIcon(type)}"></i>
            <span>${message}</span>
        </div>
        <button class="notification-close" onclick="this.parentElement.remove()">×</button>
    `;

    // Add to page
    document.body.appendChild(notification);

    // Auto remove after 5 seconds
    setTimeout(() => {
        if (notification.parentElement) {
            notification.remove();
        }
    }, 5000);

    // Add slide-in animation
    setTimeout(() => {
        notification.classList.add('show');
    }, 100);
}

function getNotificationIcon(type) {
    switch (type) {
        case 'success': return 'check-circle';
        case 'error': return 'exclamation-circle';
        case 'warning': return 'exclamation-triangle';
        default: return 'info-circle';
    }
}

// Pagination Functions
function goToPage(page) {
    showNotification(`Đang chuyển tới trang ${page}...`, 'info');

    // Update active page button
    document.querySelectorAll('.page-btn').forEach(btn => {
        btn.classList.remove('active');
        if (btn.textContent == page) {
            btn.classList.add('active');
        }
    });
}

function previousPage() {
    const activePage = document.querySelector('.page-btn.active');
    const currentPage = parseInt(activePage.textContent);
    if (currentPage > 1) {
        goToPage(currentPage - 1);
    }
}

function nextPage() {
    const activePage = document.querySelector('.page-btn.active');
    const currentPage = parseInt(activePage.textContent);
    goToPage(currentPage + 1);
}

// Keyboard shortcuts
document.addEventListener('keydown', function (e) {
    // Ctrl+N: Add new item
    if (e.ctrlKey && e.key === 'n') {
        e.preventDefault();
        showAddItemModal();
    }

    // Escape: Close modal
    if (e.key === 'Escape') {
        closeMenuItemModal();
    }

    // Ctrl+F: Focus search
    if (e.ctrlKey && e.key === 'f') {
        e.preventDefault();
        document.getElementById('menuSearchInput').focus();
    }
});

function addSize() {
    const container = document.getElementById('size-list');
    const div = document.createElement('div');
    div.className = 'form-row mb-2';
    div.innerHTML = `
        <input type="text" name="sizes[]" placeholder="Tên kích thước" class="form-control mr-2" required>
        <input type="number" name="sizePrices[]" placeholder="Giá" class="form-control mr-2" min="0">
        <button type="button" class="btn btn-danger" onclick="this.parentElement.remove()">X</button>
    `;
    container.appendChild(div);
}

function addTopping() {
    const container = document.getElementById('topping-list');
    const div = document.createElement('div');
    div.className = 'form-row mb-2';
    div.innerHTML = `
        <input type="text" name="toppings[]" placeholder="Tên topping" class="form-control mr-2" required>
        <input type="number" name="toppingPrices[]" placeholder="Giá" class="form-control mr-2" min="0">
        <button type="button" class="btn btn-danger" onclick="this.parentElement.remove()">X</button>
    `;
    container.appendChild(div);
}
