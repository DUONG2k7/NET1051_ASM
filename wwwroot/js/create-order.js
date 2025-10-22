// ===== CREATE ORDER CHECKOUT - COMPLETE JAVASCRIPT =====
let orderCart = [];
let cartItemId = 0;
let currentDateTime = new Date();
let appliedDiscount = null;
let lastAddedItem = null;

// Available discount codes
const availableDiscounts = [
    { code: 'WELCOME10', type: 'percentage', value: 10, minOrder: 50000, description: 'Giảm 10% cho đơn từ 50k' },
    { code: 'SAVE20K', type: 'fixed', value: 20000, minOrder: 100000, description: 'Giảm 20k cho đơn từ 100k' },
    { code: 'STUDENT15', type: 'percentage', value: 15, minOrder: 30000, description: 'Giảm 15% cho sinh viên' },
    { code: 'VIP5', type: 'percentage', value: 5, minOrder: 0, description: 'Giảm 5% cho khách VIP' }
];

// ===== INITIALIZATION =====
document.addEventListener('DOMContentLoaded', function () {
    initializeCreateOrder();
    setupEventListeners();
    updateCurrentDateTime();
    setInterval(updateCurrentDateTime, 1000);
    console.log('✅ Create Order Checkout initialized successfully');
});

function initializeCreateOrder() {
    updateOrderDisplay();
    updateOrderCalculations();
    validateOrder();
    initializeItemTotals();
}

function setupEventListeners() {
    // Customer form validation
    const customerInputs = document.querySelectorAll('#orderForm input, #orderForm textarea, #orderForm select');
    customerInputs.forEach(input => {
        input.addEventListener('input', debounce(validateOrder, 300));
    });

    // Menu search
    const menuSearch = document.getElementById('menuSearch');
    if (menuSearch) {
        menuSearch.addEventListener('input', debounce(searchMenu, 300));
    }

    // Extra options change
    document.addEventListener('change', function (e) {
        if (e.target.type === 'checkbox' && e.target.hasAttribute('data-extra')) {
            const itemId = getItemIdFromExtras(e.target);
            updateItemTotal(itemId);
        }
    });
}

// ===== UTILITY FUNCTIONS =====
function updateCurrentDateTime() {
    currentDateTime = new Date();
    const dateTimeElement = document.getElementById('currentDateTime');
    const orderTimeElement = document.getElementById('orderTime');

    if (dateTimeElement) {
        const options = {
            year: 'numeric',
            month: '2-digit',
            day: '2-digit',
            hour: '2-digit',
            minute: '2-digit',
            second: '2-digit',
            hour12: false
        };
        const timeString = currentDateTime.toLocaleString('vi-VN', options);
        dateTimeElement.textContent = timeString;

        if (orderTimeElement) {
            orderTimeElement.value = timeString;
        }
    }
}

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

function formatCurrency(amount) {
    return new Intl.NumberFormat('vi-VN').format(amount) + '₫';
}

function getItemIdFromExtras(checkbox) {
    const extrasSection = checkbox.closest('.extras-section');
    return extrasSection ? extrasSection.id.replace('extras', '') : null;
}

// ===== MENU FUNCTIONS =====
function filterCategory(category) {
    // Update active button
    document.querySelectorAll('.category-btn').forEach(btn => {
        btn.classList.remove('active');
    });
    event.target.classList.add('active');

    // Filter menu items
    const menuItems = document.querySelectorAll('.menu-item-card');
    menuItems.forEach(item => {
        const itemCategory = item.dataset.category;
        if (category === 'all' || itemCategory === category) {
            item.style.display = 'block';
            item.style.animation = 'fadeIn 0.3s ease';
        } else {
            item.style.display = 'none';
        }
    });
}

function searchMenu() {
    const searchTerm = document.getElementById('menuSearch').value.toLowerCase().trim();
    const menuItems = document.querySelectorAll('.menu-item-card');

    menuItems.forEach(item => {
        const itemName = item.querySelector('.item-name').textContent.toLowerCase();
        const itemDesc = item.querySelector('.item-description').textContent.toLowerCase();

        if (searchTerm === '' || itemName.includes(searchTerm) || itemDesc.includes(searchTerm)) {
            item.style.display = 'block';
        } else {
            item.style.display = 'none';
        }
    });
}

// ===== QUANTITY FUNCTIONS =====
function changeQuickQuantity(itemId, change) {
    const qtyElement = document.getElementById(`quickQty${itemId}`);
    let currentQty = parseInt(qtyElement.textContent);
    currentQty = Math.max(1, currentQty + change);
    qtyElement.textContent = currentQty;
    updateItemTotal(itemId);
}

function updateItemTotal(itemId) {
    const menuCard = document.querySelector(`.menu-item-card[data-id="${itemId}"]`);
    const basePrice = parseInt(menuCard.querySelector('.item-price').textContent.replace(/[^\d]/g, ''));
    const quantity = parseInt(document.getElementById(`quickQty${itemId}`).textContent);

    // Calculate extras
    let extrasTotal = 0;
    const extraCheckboxes = menuCard.querySelectorAll('.extra-option input[type="checkbox"]:checked');
    extraCheckboxes.forEach(checkbox => {
        extrasTotal += parseInt(checkbox.dataset.price) || 0;
    });

    const itemTotal = (basePrice + extrasTotal) * quantity;
    const itemTotalElement = document.getElementById(`itemTotal${itemId}`);
    if (itemTotalElement) {
        itemTotalElement.textContent = formatCurrency(itemTotal);
    }
}

function initializeItemTotals() {
    // Initialize all item totals
    document.querySelectorAll('.menu-item-card').forEach(card => {
        const itemId = card.dataset.id;
        updateItemTotal(itemId);
    });
}

// ===== ORDER FUNCTIONS =====
function addToOrder(itemId, itemName, basePrice, imageUrl) {
    const menuCard = document.querySelector(`.menu-item-card[data-id="${itemId}"]`);
    const addButton = menuCard.querySelector('.btn-add-to-order');

    // Show loading animation
    addButton.classList.add('item-added-animation');

    try {
        const quantity = parseInt(document.getElementById(`quickQty${itemId}`).textContent);
        const selectedExtras = [];
        let extrasTotal = 0;

        // Get selected extras
        const extraCheckboxes = menuCard.querySelectorAll('.extra-option input[type="checkbox"]:checked');
        extraCheckboxes.forEach(checkbox => {
            const extraName = checkbox.dataset.extra;
            const extraPrice = parseInt(checkbox.dataset.price) || 0;
            const extraLabel = checkbox.closest('.extra-option').querySelector('.extra-name').textContent;

            selectedExtras.push({
                id: extraName,
                name: extraLabel,
                price: extraPrice
            });
            extrasTotal += extraPrice;
        });

        const itemTotalPrice = basePrice + extrasTotal;
        const orderItem = {
            id: ++cartItemId,
            menuId: itemId,
            name: itemName,
            basePrice: basePrice,
            extras: selectedExtras,
            itemPrice: itemTotalPrice,
            quantity: quantity,
            totalPrice: itemTotalPrice * quantity,
            imageUrl: imageUrl,
            addedAt: new Date()
        };

        // Check if identical item exists
        const existingItemIndex = orderCart.findIndex(item =>
            item.menuId === itemId &&
            JSON.stringify(item.extras) === JSON.stringify(selectedExtras)
        );

        if (existingItemIndex !== -1) {
            orderCart[existingItemIndex].quantity += quantity;
            orderCart[existingItemIndex].totalPrice = orderCart[existingItemIndex].itemPrice * orderCart[existingItemIndex].quantity;
            lastAddedItem = orderCart[existingItemIndex];
            showNotification(`Đã cập nhật ${itemName} (${orderCart[existingItemIndex].quantity} món)`, 'success');
        } else {
            orderCart.push(orderItem);
            lastAddedItem = orderItem;
            showNotification(`Đã thêm ${itemName} vào đơn hàng`, 'success');
        }

        // Reset form
        document.getElementById(`quickQty${itemId}`).textContent = '1';
        extraCheckboxes.forEach(checkbox => {
            checkbox.checked = false;
        });
        updateItemTotal(itemId);

        // Show last added item
        showLastAddedItem(lastAddedItem);

        // Update displays
        updateOrderDisplay();
        updateOrderCalculations();
        validateOrder();
        updateProgressSteps();

    } catch (error) {
        console.error('Error adding item to order:', error);
        showNotification('Có lỗi xảy ra khi thêm món vào đơn hàng', 'error');
    } finally {
        setTimeout(() => {
            addButton.classList.remove('item-added-animation');
        }, 600);
    }
}

function showLastAddedItem(item) {
    const lastAddedContainer = document.getElementById('lastAddedItem');
    const lastAddedContent = document.getElementById('lastAddedContent');

    if (!lastAddedContainer || !lastAddedContent) return;

    const extrasText = item.extras.length > 0
        ? item.extras.map(extra => extra.name).join(', ')
        : 'Không có món kèm';

    lastAddedContent.innerHTML = `
        <img src="${item.imageUrl}" alt="${item.name}" class="last-added-image" onerror="this.src='/Images/placeholder.jpg'">
        <div class="last-added-info">
            <div class="last-added-name">${item.name}</div>
            <div class="last-added-extras">${extrasText}</div>
            <div class="last-added-price">${formatCurrency(item.totalPrice)}</div>
        </div>
    `;

    lastAddedContainer.style.display = 'block';

    // Auto hide after 5 seconds
    setTimeout(() => {
        lastAddedContainer.style.display = 'none';
    }, 5000);
}

function updateOrderDisplay() {
    const orderItemsContainer = document.getElementById('orderItems');

    if (orderCart.length === 0) {
        orderItemsContainer.innerHTML = `
            <div class="empty-order">
                <div class="empty-order-icon">
                    <i class="fas fa-utensils"></i>
                </div>
                <p>Chưa có món nào</p>
                <small class="text-muted">Chọn món từ menu bên trái</small>
            </div>
        `;
        return;
    }

    let orderHTML = '';
    orderCart.forEach(item => {
        const extrasText = item.extras.length > 0
            ? item.extras.map(extra => extra.name).join(', ')
            : 'Không có món kèm';

        orderHTML += `
            <div class="summary-item" data-item-id="${item.id}">
                <img src="${item.imageUrl}" alt="${item.name}" onerror="this.src='/Images/placeholder.jpg'">
                <div class="item-info">
                    <h4>${item.name}</h4>
                    <div class="item-options">${extrasText}</div>
                    <div class="item-quantity">Số lượng: ${item.quantity}</div>
                </div>
                <div class="item-price-display">${formatCurrency(item.totalPrice)}</div>
                <div class="item-controls">
                    <button class="btn-qty-control" onclick="updateOrderItemQuantity(${item.id}, -1)" title="Giảm">
                        <i class="fas fa-minus"></i>
                    </button>
                    <span class="item-qty-display">${item.quantity}</span>
                    <button class="btn-qty-control" onclick="updateOrderItemQuantity(${item.id}, 1)" title="Tăng">
                        <i class="fas fa-plus"></i>
                    </button>
                    <button class="btn-remove-item" onclick="removeOrderItem(${item.id})" title="Xóa">
                        <i class="fas fa-trash"></i>
                    </button>
                </div>
            </div>
        `;
    });

    orderItemsContainer.innerHTML = orderHTML;
}

function updateOrderItemQuantity(itemId, change) {
    const itemIndex = orderCart.findIndex(item => item.id === itemId);
    if (itemIndex !== -1) {
        const item = orderCart[itemIndex];
        item.quantity += change;

        if (item.quantity <= 0) {
            orderCart.splice(itemIndex, 1);
            showNotification(`Đã xóa ${item.name} khỏi đơn hàng`, 'info');
        } else {
            item.totalPrice = item.itemPrice * item.quantity;
            showNotification(`Đã cập nhật ${item.name} (${item.quantity} món)`, 'success');
        }

        updateOrderDisplay();
        updateOrderCalculations();
        validateOrder();
        updateProgressSteps();
    }
}

function removeOrderItem(itemId) {
    const itemIndex = orderCart.findIndex(item => item.id === itemId);
    if (itemIndex !== -1) {
        const itemName = orderCart[itemIndex].name;
        orderCart.splice(itemIndex, 1);

        updateOrderDisplay();
        updateOrderCalculations();
        validateOrder();
        updateProgressSteps();

        showNotification(`Đã xóa ${itemName} khỏi đơn hàng`, 'info');
    }
}

// ===== ORDER CALCULATIONS =====
function updateOrderCalculations() {
    const calculationsContainer = document.getElementById('orderCalculations');

    if (orderCart.length === 0) {
        calculationsContainer.style.display = 'none';
        return;
    }

    calculationsContainer.style.display = 'block';

    const subtotal = orderCart.reduce((total, item) => total + item.totalPrice, 0);
    let discountAmount = 0;

    // Calculate discount
    if (appliedDiscount && subtotal >= appliedDiscount.minOrder) {
        if (appliedDiscount.type === 'percentage') {
            discountAmount = Math.floor(subtotal * appliedDiscount.value / 100);
        } else {
            discountAmount = appliedDiscount.value;
        }
    }

    const finalTotal = subtotal - discountAmount;
    const totalItems = orderCart.reduce((total, item) => total + item.quantity, 0);

    // Update display
    document.getElementById('totalItems').textContent = totalItems;
    document.getElementById('subtotal').textContent = formatCurrency(subtotal);
    document.getElementById('finalTotal').textContent = formatCurrency(finalTotal);
    document.getElementById('orderTotalDisplay').textContent = formatCurrency(finalTotal);

    // Show/hide discount row
    const discountRow = document.getElementById('discountRow');
    if (discountAmount > 0) {
        discountRow.style.display = 'flex';
        document.getElementById('discountAmount').textContent = '-' + formatCurrency(discountAmount);
        document.getElementById('discountCodeDisplay').textContent = appliedDiscount.code;
    } else {
        discountRow.style.display = 'none';
    }
}

// ===== DISCOUNT FUNCTIONS =====
function applyDiscountCode() {
    const discountCodeInput = document.getElementById('discountCode');
    const discountCode = discountCodeInput.value.trim().toUpperCase();

    if (!discountCode) {
        showNotification('Vui lòng nhập mã giảm giá', 'warning');
        return;
    }

    const subtotal = orderCart.reduce((total, item) => total + item.totalPrice, 0);

    if (subtotal === 0) {
        showNotification('Vui lòng chọn món trước khi áp dụng mã giảm giá', 'warning');
        return;
    }

    // Find discount
    const discount = availableDiscounts.find(d => d.code === discountCode);

    if (!discount) {
        showNotification('Mã giảm giá không hợp lệ', 'error');
        return;
    }

    if (subtotal < discount.minOrder) {
        showNotification(`Đơn hàng tối thiểu ${formatCurrency(discount.minOrder)} để sử dụng mã này`, 'warning');
        return;
    }

    if (appliedDiscount && appliedDiscount.code === discountCode) {
        showNotification('Mã giảm giá này đã được áp dụng', 'info');
        return;
    }

    // Apply discount
    appliedDiscount = discount;
    discountCodeInput.value = '';

    // Show applied discount
    const appliedDiscountElement = document.getElementById('appliedDiscount');
    document.getElementById('appliedDiscountCode').textContent = discount.code;
    document.getElementById('appliedDiscountDesc').textContent =
        discount.type === 'percentage' ? `Giảm ${discount.value}%` : `Giảm ${formatCurrency(discount.value)}`;
    appliedDiscountElement.style.display = 'block';

    updateOrderCalculations();
    showNotification(`Đã áp dụng mã giảm giá ${discount.code}`, 'success');
}

function removeDiscount() {
    appliedDiscount = null;
    document.getElementById('appliedDiscount').style.display = 'none';
    updateOrderCalculations();
    showNotification('Đã hủy mã giảm giá', 'info');
}

// ===== VALIDATION =====
function validateOrder() {
    const customerName = document.getElementById('customerName')?.value.trim() || '';
    const customerPhone = document.getElementById('customerPhone')?.value.trim() || '';

    const hasItems = orderCart.length > 0;
    const hasCustomerName = customerName.length > 0;
    const hasCustomerPhone = customerPhone.length > 0;

    const isValid = hasItems && hasCustomerName && hasCustomerPhone;

    const confirmBtn = document.getElementById('confirmOrderBtn');
    const validationMessage = document.getElementById('validationMessage');

    if (confirmBtn) confirmBtn.disabled = !isValid;

    // Update validation message
    if (validationMessage) {
        if (!hasItems) {
            validationMessage.textContent = 'Vui lòng chọn món ăn';
        } else if (!hasCustomerName) {
            validationMessage.textContent = 'Vui lòng nhập tên khách hàng';
        } else if (!hasCustomerPhone) {
            validationMessage.textContent = 'Vui lòng nhập số điện thoại';
        } else {
            validationMessage.innerHTML = '<i class="fas fa-check-circle text-success"></i> Sẵn sàng xác nhận đơn hàng';
        }
    }

    return isValid;
}

// ===== PROGRESS STEPS =====
function updateProgressSteps() {
    const steps = document.querySelectorAll('.step');

    // Reset all steps
    steps.forEach(step => {
        step.classList.remove('completed', 'active');
    });

    // Step 1: Customer info
    const hasCustomerInfo = document.getElementById('customerName')?.value.trim() &&
        document.getElementById('customerPhone')?.value.trim();

    if (hasCustomerInfo) {
        steps[0].classList.add('completed');
        steps[1].classList.add('active');
    } else {
        steps[0].classList.add('active');
    }

    // Step 2: Menu selection
    if (orderCart.length > 0) {
        steps[1].classList.remove('active');
        steps[1].classList.add('completed');
        steps[2].classList.add('active');
    }

    // Step 3: Ready to confirm
    if (validateOrder()) {
        steps[2].classList.remove('active');
        steps[2].classList.add('completed');
        steps[3].classList.add('active');
    }
}

// ===== ORDER ACTIONS =====
function clearAllOrder() {
    if (orderCart.length === 0 && !hasCustomerData()) {
        showNotification('Không có gì để xóa', 'info');
        return;
    }

    if (confirm('Bạn có chắc muốn xóa toàn bộ đơn hàng và thông tin khách hàng?')) {
        // Clear everything
        orderCart = [];
        lastAddedItem = null;
        appliedDiscount = null;

        // Reset form
        const orderForm = document.getElementById('orderForm');
        if (orderForm) orderForm.reset();

        // Update time
        updateCurrentDateTime();

        // Reset quantities and extras
        document.querySelectorAll('.quick-quantity').forEach(qty => {
            qty.textContent = '1';
        });

        document.querySelectorAll('.extra-option input[type="checkbox"]').forEach(checkbox => {
            checkbox.checked = false;
        });

        // Reset item totals
        initializeItemTotals();

        // Hide elements
        document.getElementById('lastAddedItem').style.display = 'none';
        document.getElementById('appliedDiscount').style.display = 'none';

        // Update displays
        updateOrderDisplay();
        updateOrderCalculations();
        validateOrder();
        updateProgressSteps();

        showNotification('Đã xóa toàn bộ đơn hàng', 'success');
    }
}

function hasCustomerData() {
    const customerName = document.getElementById('customerName')?.value.trim() || '';
    const customerPhone = document.getElementById('customerPhone')?.value.trim() || '';
    const orderNote = document.getElementById('orderNote')?.value.trim() || '';

    return customerName || customerPhone || orderNote;
}

// Submit order
document.getElementById('orderForm')?.addEventListener('submit', function (e) {
    e.preventDefault();
    confirmOrder();
});

function confirmOrder() {
    if (!validateOrder()) {
        showNotification('Vui lòng kiểm tra lại thông tin đơn hàng', 'warning');
        return;
    }

    const confirmBtn = document.getElementById('confirmOrderBtn');
    const originalHTML = confirmBtn.innerHTML;

    // Show loading state
    confirmBtn.disabled = true;
    confirmBtn.innerHTML = `
        <i class="fas fa-spinner fa-spin"></i>
        <span class="confirm-text">Đang xử lý...</span>
        <span class="order-total-display">Vui lòng đợi</span>
    `;

    // Calculate totals
    const subtotal = orderCart.reduce((total, item) => total + item.totalPrice, 0);
    let discountAmount = 0;

    if (appliedDiscount && subtotal >= appliedDiscount.minOrder) {
        if (appliedDiscount.type === 'percentage') {
            discountAmount = Math.floor(subtotal * appliedDiscount.value / 100);
        } else {
            discountAmount = appliedDiscount.value;
        }
    }

    const finalTotal = subtotal - discountAmount;

    // Prepare order data
    const orderData = {
        id: 'ORD' + Date.now(),
        customer: {
            name: document.getElementById('customerName').value.trim(),
            phone: document.getElementById('customerPhone').value.trim(),
            tableNumber: document.getElementById('tableNumber').value,
            note: document.getElementById('orderNote').value.trim()
        },
        orderTime: currentDateTime,
        items: orderCart.map(item => ({
            menuId: item.menuId,
            name: item.name,
            quantity: item.quantity,
            basePrice: item.basePrice,
            extras: item.extras,
            totalPrice: item.totalPrice
        })),
        discount: appliedDiscount,
        summary: {
            subtotal: subtotal,
            discountAmount: discountAmount,
            finalTotal: finalTotal,
            totalItems: orderCart.reduce((total, item) => total + item.quantity, 0)
        },
        createdAt: currentDateTime,
        staff: 'Current Staff'
    };

    // Simulate API call
    setTimeout(() => {
        try {
            // Success
            showOrderSuccessModal(orderData);

            // Reset everything
            clearAllOrder();

        } catch (error) {
            console.error('Order confirmation error:', error);
            showNotification('Có lỗi xảy ra khi xử lý đơn hàng. Vui lòng thử lại.', 'error');
        } finally {
            // Reset button
            confirmBtn.disabled = false;
            confirmBtn.innerHTML = originalHTML;
        }
    }, 2000);
}

// ===== SUCCESS MODAL =====
function showOrderSuccessModal(orderData) {
    const modal = document.createElement('div');
    modal.className = 'modal fade show';
    modal.style.cssText = 'display: block; background: rgba(0,0,0,0.5); z-index: 10000;';

    modal.innerHTML = `
        <div class="modal-dialog modal-dialog-centered modal-lg">
            <div class="modal-content">
                <div class="modal-header bg-success text-white">
                    <h4 class="modal-title">
                        <i class="fas fa-check-circle"></i>
                        Đặt hàng thành công!
                    </h4>
                </div>
                <div class="modal-body">
                    <div class="row">
                        <div class="col-md-6">
                            <div class="text-center mb-4">
                                <div class="success-icon" style="font-size: 3rem; color: #28a745; margin-bottom: 15px;">
                                    <i class="fas fa-check-circle"></i>
                                </div>
                                <h3 class="text-success">Thành công!</h3>
                                <p class="mb-2">Mã đơn hàng:</p>
                                <h4 class="text-primary">${orderData.id}</h4>
                            </div>
                        </div>
                        <div class="col-md-6">
                            <div class="order-summary-modal">
                                <h5>Thông tin đơn hàng:</h5>
                                <div class="summary-row">
                                    <span>Khách hàng:</span>
                                    <strong>${orderData.customer.name}</strong>
                                </div>
                                <div class="summary-row">
                                    <span>SĐT:</span>
                                    <strong>${orderData.customer.phone}</strong>
                                </div>
                                ${orderData.customer.tableNumber ? `
                                <div class="summary-row">
                                    <span>Bàn số:</span>
                                    <strong>${orderData.customer.tableNumber}</strong>
                                </div>
                                ` : ''}
                                <hr>
                                <div class="summary-row">
                                    <span>Tạm tính:</span>
                                    <span>${formatCurrency(orderData.summary.subtotal)}</span>
                                </div>
                                ${orderData.summary.discountAmount > 0 ? `
                                <div class="summary-row text-success">
                                    <span>Giảm giá (${orderData.discount.code}):</span>
                                    <span>-${formatCurrency(orderData.summary.discountAmount)}</span>
                                </div>
                                ` : ''}
                                <div class="summary-row total-row">
                                    <span><strong>Tổng cộng:</strong></span>
                                    <strong class="text-success">${formatCurrency(orderData.summary.finalTotal)}</strong>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button class="btn btn-outline-info" onclick="printOrder('${orderData.id}')">
                        <i class="fas fa-print"></i>
                        In hóa đơn
                    </button>
                    <button class="btn btn-primary" onclick="closeSuccessModal()">
                        <i class="fas fa-plus"></i>
                        Tạo đơn mới
                    </button>
                    <button class="btn btn-outline-secondary" onclick="goToDashboard()">
                        <i class="fas fa-tachometer-alt"></i>
                        Dashboard
                    </button>
                </div>
            </div>
        </div>
    `;

    document.body.appendChild(modal);

    // Auto close after 15 seconds
    setTimeout(() => {
        if (document.body.contains(modal)) {
            closeSuccessModal();
        }
    }, 15000);
}

function closeSuccessModal() {
    const modal = document.querySelector('.modal.show');
    if (modal) {
        document.body.removeChild(modal);
    }
}

function printOrder(orderId) {
    showNotification('Tính năng in hóa đơn sẽ được cập nhật', 'info');
    console.log(`Print order: ${orderId}`);
}

function goToDashboard() {
    window.location.href = '/sales-staff/dashboard';
}

// ===== UTILITY FUNCTIONS =====
function searchExistingCustomer() {
    showNotification('Tính năng tìm kiếm khách hàng sẽ được cập nhật', 'info');
}

// ===== NOTIFICATION SYSTEM =====
function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `alert alert-${type} notification-toast`;
    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        z-index: 9999;
        min-width: 300px;
        max-width: 400px;
        animation: slideInRight 0.3s ease;
        box-shadow: 0 4px 12px rgba(0,0,0,0.15);
        border: none;
        border-radius: 8px;
    `;
    notification.innerHTML = `
        <div style="display: flex; align-items: center; gap: 10px;">
            <i class="fas fa-${getNotificationIcon(type)}"></i>
            <span style="flex: 1;">${message}</span>
            <button type="button" class="close" onclick="this.parentElement.parentElement.remove()" style="background: none; border: none; font-size: 1.2rem; cursor: pointer; padding: 0;">
                <span>&times;</span>
            </button>
        </div>
    `;

    document.body.appendChild(notification);

    // Auto remove after 4 seconds
    setTimeout(() => {
        if (document.body.contains(notification)) {
            notification.style.animation = 'slideOutRight 0.3s ease';
            setTimeout(() => {
                if (document.body.contains(notification)) {
                    document.body.removeChild(notification);
                }
            }, 300);
        }
    }, 4000);
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

// ===== CSS ANIMATIONS =====
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from { transform: translateX(100%); opacity: 0; }
        to { transform: translateX(0); opacity: 1; }
    }
    
    @keyframes slideOutRight {
        from { transform: translateX(0); opacity: 1; }
        to { transform: translateX(100%); opacity: 0; }
    }
    
    @keyframes fadeIn {
        from { opacity: 0; transform: translateY(10px); }
        to { opacity: 1; transform: translateY(0); }
    }
    
    .order-summary-modal {
        background: #f8f9fa;
        padding: 15px;
        border-radius: 8px;
        font-size: 0.9rem;
    }
    
    .summary-row {
        display: flex;
        justify-content: space-between;
        margin-bottom: 8px;
    }
    
    .summary-row.total-row {
        border-top: 2px solid #28a745;
        padding-top: 10px;
        margin-top: 10px;
        font-size: 1.1rem;
    }
`;
document.head.appendChild(style);

console.log('✅ Create Order Checkout JavaScript loaded successfully');


        // ===== VALIDATION MESSAGES =====
function setupValidationMessages() {
    const validationRules = {
        'customerName': {
            required: true,
            message: 'Vui lòng nhập tên khách hàng'
        },
        'customerPhone': {
            required: true,
            pattern: /^[0-9]{10,11}$/,
            message: 'Vui lòng nhập số điện thoại hợp lệ (10-11 số)'
        },
        'orderNote': {
            required: false,
            message: ''
        }
    };

    Object.keys(validationRules).forEach(fieldId => {
        const field = document.getElementById(fieldId);
        if (field) {
            setupFieldValidation(field, validationRules[fieldId]);
        }
    });
}

function setupFieldValidation(field, rules) {
    const formGroup = field.closest('.form-group');
    let validationMessage = formGroup.querySelector('.validation-message');
    
    if (!validationMessage) {
        validationMessage = document.createElement('div');
        validationMessage.className = 'validation-message';
        formGroup.appendChild(validationMessage);
    }

    field.addEventListener('input', function() {
        validateField(field, rules, validationMessage);
    });

    field.addEventListener('blur', function() {
        validateField(field, rules, validationMessage);
    });
}

function validateField(field, rules, validationMessage) {
    const value = field.value.trim();
    
    if (rules.required && !value) {
        showValidationMessage(validationMessage, rules.message, 'error');
        field.classList.add('is-invalid');
        field.classList.remove('is-valid');
        return false;
    }
    
    if (value && rules.pattern && !rules.pattern.test(value)) {
        showValidationMessage(validationMessage, rules.message, 'error');
        field.classList.add('is-invalid');
        field.classList.remove('is-valid');
        return false;
    }
    
    if (value || !rules.required) {
        if (rules.required && value) {
            showValidationMessage(validationMessage, 'Hợp lệ', 'success');
        } else {
            hideValidationMessage(validationMessage);
        }
        field.classList.remove('is-invalid');
        field.classList.add('is-valid');
        return true;
    }
    
    return true;
}

function showValidationMessage(element, message, type) {
    element.innerHTML = `<i class="fas fa-${type === 'error' ? 'exclamation-circle' : 'check-circle'}"></i> ${message}`;
    element.className = `validation-message ${type} show`;
}

function hideValidationMessage(element) {
    element.className = 'validation-message';
    element.innerHTML = '';
}

// ===== UPDATE PROGRESS STEPS - 3 STEPS ===== 
function updateProgressSteps() {
    const steps = document.querySelectorAll('.step');
    
    // Reset all steps
    steps.forEach(step => {
        step.classList.remove('completed', 'active');
    });
    
    // Step 1: Customer info (always active initially)
    const hasCustomerInfo = document.getElementById('customerName')?.value.trim() && 
                           document.getElementById('customerPhone')?.value.trim();
    
    if (hasCustomerInfo && orderCart.length > 0) {
        steps[0].classList.add('completed');
        steps[1].classList.add('active');
    } else {
        steps[0].classList.add('active');
    }
    
    // Step 2: Ready to confirm
    if (validateOrder()) {
        steps[1].classList.remove('active');
        steps[1].classList.add('completed');
        steps[2].classList.add('active');
    }
}

// ===== ENHANCED INITIALIZATION =====
document.addEventListener('DOMContentLoaded', function() {
    initializeCreateOrder();
    setupEventListeners();
    setupValidationMessages(); // Add this line
    updateCurrentDateTime();
    setInterval(updateCurrentDateTime, 1000);
    console.log('✅ Create Order Checkout with validation initialized successfully');
});
