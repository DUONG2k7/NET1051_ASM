// Sticky Navigation & Scroll Effects
document.addEventListener('DOMContentLoaded', function () {
    const header = document.querySelector('.header-nav');
    const mobileToggle = document.querySelector('.mobile-menu-toggle');
    const navMenu = document.querySelector('.nav-menu');

    // Sticky navigation với hiệu ứng
    window.addEventListener('scroll', function () {
        if (window.scrollY > 100) {
            header.classList.add('scrolled');
        } else {
            header.classList.remove('scrolled');
        }
    });

    // Mobile menu toggle
    if (mobileToggle) {
        mobileToggle.addEventListener('click', function () {
            navMenu.classList.toggle('mobile-active');
            this.classList.toggle('active');
        });
    }

    // Smooth scroll cho internal links
    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
        anchor.addEventListener('click', function (e) {
            e.preventDefault();
            const target = document.querySelector(this.getAttribute('href'));
            if (target) {
                target.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // Animate elements on scroll
    const observerOptions = {
        threshold: 0.1,
        rootMargin: '0px 0px -50px 0px'
    };

    const observer = new IntersectionObserver(function (entries) {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in-up');
            }
        });
    }, observerOptions);

    // Observe elements for animation
    document.querySelectorAll('.menu-item-card, .footer-section').forEach(el => {
        observer.observe(el);
    });
});

// Enhanced cart functions
function viewDetail(slug) {
    window.location.href = `/food/${slug}`;
}

function addToCart(foodId) {
    // Hiệu ứng thêm vào giỏ hàng
    const cartBtn = document.querySelector('.cart-btn');
    cartBtn.style.transform = 'scale(1.1)';
    cartBtn.style.background = 'var(--success-color)';

    setTimeout(() => {
        cartBtn.style.transform = '';
        cartBtn.style.background = '';
    }, 300);

    showNotification('Đã thêm món vào giỏ hàng!', 'success');
    //updateCartCount();
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    notification.className = `notification ${type}`;
    notification.innerHTML = `
        <i class="fas fa-check-circle"></i>
        <span>${message}</span>
    `;

    notification.style.cssText = `
        position: fixed;
        top: 20px;
        right: 20px;
        padding: 16px 20px;
        background: ${type === 'success' ? 'var(--success-color)' : 'var(--primary-color)'};
        color: white;
        border-radius: var(--border-radius);
        z-index: 10000;
        display: flex;
        align-items: center;
        gap: 8px;
        box-shadow: var(--shadow-hover);
        animation: slideInRight 0.3s ease;
        font-weight: 500;
    `;

    document.body.appendChild(notification);

    setTimeout(() => {
        notification.style.animation = 'slideOutRight 0.3s ease';
        setTimeout(() => notification.remove(), 300);
    }, 3000);
}

function updateCartCount() {
    const cartCount = document.querySelector('.cart-count');
    if (cartCount) {
        let currentCount = parseInt(cartCount.textContent) || 0;
        cartCount.textContent = currentCount + 1;

        // Animation hiệu ứng
        cartCount.style.animation = 'bounce 0.5s ease';
        setTimeout(() => {
            cartCount.style.animation = '';
        }, 500);
    }
}

// CSS animations
const style = document.createElement('style');
style.textContent = `
    @keyframes slideInRight {
        from {
            transform: translateX(100%);
            opacity: 0;
        }
        to {
            transform: translateX(0);
            opacity: 1;
        }
    }
    
    @keyframes slideOutRight {
        from {
            transform: translateX(0);
            opacity: 1;
        }
        to {
            transform: translateX(100%);
            opacity: 0;
        }
    }
    
    @keyframes bounce {
        0%, 20%, 53%, 80%, 100% {
            transform: scale(1);
        }
        40%, 43% {
            transform: scale(1.3);
        }
        70% {
            transform: scale(1.1);
        }
        90% {
            transform: scale(1.05);
        }
    }
    
    .mobile-menu-toggle.active span:nth-child(1) {
        transform: rotate(45deg) translate(5px, 5px);
    }
    
    .mobile-menu-toggle.active span:nth-child(2) {
        opacity: 0;
    }
    
    .mobile-menu-toggle.active span:nth-child(3) {
        transform: rotate(-45deg) translate(7px, -6px);
    }
    
    @media (max-width: 768px) {
        .nav-menu.mobile-active {
            display: flex;
            flex-direction: column;
            position: absolute;
            top: 100%;
            left: 0;
            right: 0;
            background: white;
            box-shadow: var(--shadow-hover);
            padding: 20px;
            gap: 8px;
        }
    }
`;
document.head.appendChild(style);


// Food Detail Page Functions
let basePrice = 45000;
let currentPrice = basePrice;

// Change main image
function changeImage(thumbnail) {
    const mainImage = document.getElementById('mainImage');
    mainImage.src = thumbnail.src;

    // Update active thumbnail
    document.querySelectorAll('.thumb').forEach(thumb => {
        thumb.classList.remove('active');
    });
    thumbnail.classList.add('active');
}

// Change quantity
function changeQuantity(change) {
    const quantityInput = document.getElementById('quantity');
    let currentQuantity = parseInt(quantityInput.value);
    let newQuantity = currentQuantity + change;

    if (newQuantity >= 1 && newQuantity <= 10) {
        quantityInput.value = newQuantity;
        updateTotalPrice();
    }
}

// Update total price
function updateTotalPrice() {
    const quantity = parseInt(document.getElementById('quantity').value);

    // Calculate size price
    const activeSizeBtn = document.querySelector('.option-btn.active');
    const sizePrice = parseInt(activeSizeBtn.dataset.price);

    // Calculate toppings price
    let toppingsPrice = 0;
    const checkedToppings = document.querySelectorAll('.topping-item input:checked');
    checkedToppings.forEach(topping => {
        toppingsPrice += parseInt(topping.dataset.price);
    });

    const totalPrice = (sizePrice + toppingsPrice) * quantity;
    document.getElementById('totalPrice').textContent = formatPrice(totalPrice);
    currentPrice = totalPrice;
}

// Format price
function formatPrice(price) {
    return new Intl.NumberFormat('vi-VN').format(price) + ' VNĐ';
}

// Size option selection
//document.addEventListener('DOMContentLoaded', function () {
//    // Size buttons
//    document.querySelectorAll('.option-btn').forEach(btn => {
//        btn.addEventListener('click', function () {
//            document.querySelectorAll('.option-btn').forEach(b => b.classList.remove('active'));
//            this.classList.add('active');
//            updateTotalPrice();
//        });
//    });

//    // Topping checkboxes
//    document.querySelectorAll('.topping-item input').forEach(checkbox => {
//        checkbox.addEventListener('change', updateTotalPrice);
//    });

//    // Quantity input change
//    document.getElementById('quantity').addEventListener('change', updateTotalPrice);
//});

// Tab switching
function showTab(tabName) {
    // Hide all tab contents
    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.remove('active');
    });

    // Remove active class from all tab buttons
    document.querySelectorAll('.tab-btn').forEach(btn => {
        btn.classList.remove('active');
    });

    // Show selected tab content
    document.getElementById(tabName).classList.add('active');

    // Add active class to clicked tab button
    event.target.classList.add('active');
}

// Add to cart from detail page
function addToCartDetail() {
    const quantity = parseInt(document.getElementById('quantity').value);
    const specialNote = document.querySelector('.special-note').value;

    // Get selected options
    const selectedSize = document.querySelector('.option-btn.active').textContent;
    const selectedToppings = [];
    document.querySelectorAll('.topping-item input:checked').forEach(topping => {
        const toppingName = topping.parentElement.querySelector('.topping-name').textContent;
        selectedToppings.push(toppingName);
    });

    // Create order details
    const orderDetails = {
        quantity: quantity,
        size: selectedSize,
        toppings: selectedToppings,
        note: specialNote,
        totalPrice: currentPrice
    };

    console.log('Order Details:', orderDetails);

    showNotification(`Đã thêm ${quantity} món vào giỏ hàng!`, 'success');
    updateCartCount(quantity);

    // Scroll to top
    window.scrollTo({ top: 0, behavior: 'smooth' });
}

// Order now
function orderNow() {
    addToCartDetail();

    // Redirect to cart page after a short delay
    setTimeout(() => {
        window.location.href = '/cart';
    }, 1000);
}

// Quick add related product
//function quickAdd(foodId) {
//    showNotification('Đã thêm món liên quan vào giỏ hàng!', 'success');
//    updateCartCount(1);
//}

// Update cart count with specific quantity
function updateCartCount(addQuantity = 1) {
    const cartCount = document.querySelector('.cart-count');
    if (cartCount) {
        let currentCount = parseInt(cartCount.textContent) || 0;
        cartCount.textContent = currentCount + addQuantity;

        // Animation effect
        cartCount.style.animation = 'bounce 0.5s ease';
        setTimeout(() => {
            cartCount.style.animation = '';
        }, 500);
    }
}


// User Profile Functions
function showTab(tabName) {
    // Hide all tab contents
    document.querySelectorAll('.tab-content').forEach(content => {
        content.classList.remove('active');
    });

    // Remove active class from all nav items
    document.querySelectorAll('.nav-item').forEach(item => {
        item.classList.remove('active');
    });

    // Show selected tab content
    document.getElementById(tabName).classList.add('active');

    // Add active class to clicked nav item
    event.target.closest('.nav-item').classList.add('active');
}

// Profile editing functions
function toggleEdit() {
    const inputs = document.querySelectorAll('#profileForm input');
    const formActions = document.getElementById('formActions');
    const editBtn = document.querySelector('.btn-edit');

    inputs.forEach(input => {
        if (input.hasAttribute('readonly')) {
            input.removeAttribute('readonly');
            input.style.background = 'white';
        } else {
            input.setAttribute('readonly', true);
            input.style.background = '#f8f9fa';
        }
    });

    if (formActions.style.display === 'none') {
        formActions.style.display = 'flex';
        editBtn.innerHTML = '<i class="fas fa-times"></i> Hủy';
    } else {
        formActions.style.display = 'none';
        editBtn.innerHTML = '<i class="fas fa-edit"></i> Chỉnh sửa';
    }
}

function cancelEdit() {
    toggleEdit();
    // Reset form values here if needed
}

// Order functions
function filterOrders(status) {
    const orders = document.querySelectorAll('.order-item');
    const filterBtns = document.querySelectorAll('.filter-btn');

    // Update active filter button
    filterBtns.forEach(btn => btn.classList.remove('active'));
    event.target.classList.add('active');

    // Filter orders
    orders.forEach(order => {
        if (status === 'all' || order.dataset.status === status) {
            order.style.display = 'block';
        } else {
            order.style.display = 'none';
        }
    });
}

function reorder(orderId) {
    showNotification('Đã thêm các món từ đơn hàng vào giỏ hàng!', 'success');
    // Add reorder logic here
}

function trackOrder(orderId) {
    showNotification('Đang theo dõi đơn hàng #DH' + orderId.toString().padStart(3, '0'), 'info');
    // Add tracking logic here
}

function cancelOrder(orderId) {
    if (confirm('Bạn có chắc muốn hủy đơn hàng này?')) {
        showNotification('Đã hủy đơn hàng thành công', 'success');
        // Add cancel logic here
    }
}

// Review functions
function showReviewModal(orderId) {
    const modal = document.getElementById('reviewModal');
    modal.classList.add('active');
    // Load order details for review
}

function closeReviewModal() {
    const modal = document.getElementById('reviewModal');
    modal.classList.remove('active');

    // Reset form
    document.querySelectorAll('.star-rating i').forEach(star => {
        star.classList.remove('active');
        star.classList.add('far');
        star.classList.remove('fas');
    });
    document.querySelector('.review-text textarea').value = '';
}

function submitReview() {
    const rating = document.querySelectorAll('.star-rating i.active').length;
    const reviewText = document.querySelector('.review-text textarea').value;

    if (rating === 0) {
        showNotification('Vui lòng chọn số sao đánh giá', 'warning');
        return;
    }

    if (!reviewText.trim()) {
        showNotification('Vui lòng nhập nội dung đánh giá', 'warning');
        return;
    }

    // Submit review logic here
    showNotification('Đã gửi đánh giá thành công!', 'success');
    closeReviewModal();
}

function editReview(reviewId) {
    // Edit review logic
    showNotification('Chức năng sửa đánh giá sẽ được cập nhật', 'info');
}

function deleteReview(reviewId) {
    if (confirm('Bạn có chắc muốn xóa đánh giá này?')) {
        showNotification('Đã xóa đánh giá', 'success');
        // Delete review logic here
    }
}

// Star rating interaction
//document.addEventListener('DOMContentLoaded', function () {
//    const stars = document.querySelectorAll('.star-rating i');

//    stars.forEach((star, index) => {
//        star.addEventListener('click', function () {
//            const rating = parseInt(this.dataset.rating);

//            stars.forEach((s, i) => {
//                if (i < rating) {
//                    s.classList.add('active', 'fas');
//                    s.classList.remove('far');
//                } else {
//                    s.classList.remove('active', 'fas');
//                    s.classList.add('far');
//                }
//            });
//        });

//        star.addEventListener('mouseover', function () {
//            const rating = parseInt(this.dataset.rating);

//            stars.forEach((s, i) => {
//                if (i < rating) {
//                    s.classList.add('fas');
//                    s.classList.remove('far');
//                } else {
//                    s.classList.remove('fas');
//                    s.classList.add('far');
//                }
//            });
//        });
//    });

//    // Reset on mouse leave
//    document.querySelector('.star-rating').addEventListener('mouseleave', function () {
//        stars.forEach(star => {
//            if (!star.classList.contains('active')) {
//                star.classList.add('far');
//                star.classList.remove('fas');
//            }
//        });
//    });
//});

// Avatar change function
function changeAvatar() {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = 'image/*';

    input.onchange = function (e) {
        const file = e.target.files[0];
        if (file) {
            const reader = new FileReader();
            reader.onload = function (e) {
                document.getElementById('userAvatar').src = e.target.result;
                showNotification('Đã cập nhật ảnh đại diện!', 'success');
            };
            reader.readAsDataURL(file);
        }
    };

    input.click();
}

// Admin Panel JavaScript Functions

// Sidebar toggle
function toggleSidebar() {
    const sidebar = document.querySelector('.admin-sidebar');
    const main = document.querySelector('.admin-main');

    sidebar.classList.toggle('collapsed');
    main.classList.toggle('expanded');
}

// Mobile sidebar toggle
function toggleMobileSidebar() {
    const sidebar = document.querySelector('.admin-sidebar');
    sidebar.classList.toggle('show');
}

// Dashboard functions
document.addEventListener('DOMContentLoaded', function () {
    // Handle mobile menu
    const menuToggle = document.querySelector('.menu-toggle');
    if (menuToggle) {
        menuToggle.addEventListener('click', function () {
            if (window.innerWidth <= 768) {
                toggleMobileSidebar();
            } else {
                toggleSidebar();
            }
        });
    }

    // Set active nav item based on current URL
    const currentPath = window.location.pathname;
    const navItems = document.querySelectorAll('.nav-item');

    navItems.forEach(item => {
        if (item.getAttribute('href') === currentPath) {
            item.classList.add('active');
        }
    });

    // Initialize charts if Chart.js is available
    if (typeof Chart !== 'undefined') {
        initializeCharts();
    }
});

// Chart initialization
function initializeCharts() {
    // Orders Chart
    const ordersCtx = document.getElementById('ordersChart');
    if (ordersCtx) {
        new Chart(ordersCtx, {
            type: 'line',
            data: {
                labels: ['T2', 'T3', 'T4', 'T5', 'T6', 'T7', 'CN'],
                datasets: [{
                    label: 'Đơn hàng',
                    data: [65, 59, 80, 81, 56, 89, 67],
                    borderColor: '#e74c3c',
                    backgroundColor: 'rgba(231, 76, 60, 0.1)',
                    tension: 0.4
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }

    // Users Chart
    const usersCtx = document.getElementById('usersChart');
    if (usersCtx) {
        new Chart(usersCtx, {
            type: 'doughnut',
            data: {
                labels: ['Quận 1', 'Quận 3', 'Quận 7', 'Khác'],
                datasets: [{
                    data: [35, 25, 20, 20],
                    backgroundColor: [
                        '#e74c3c',
                        '#3498db',
                        '#2ecc71',
                        '#95a5a6'
                    ]
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }

    // Category Chart (Food Admin)
    const categoryCtx = document.getElementById('categoryChart');
    if (categoryCtx) {
        new Chart(categoryCtx, {
            type: 'pie',
            data: {
                labels: ['Cơm', 'Phở', 'Bánh mì', 'Đồ uống', 'Khác'],
                datasets: [{
                    data: [40, 25, 15, 12, 8],
                    backgroundColor: [
                        '#e74c3c',
                        '#3498db',
                        '#2ecc71',
                        '#f39c12',
                        '#95a5a6'
                    ]
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        position: 'bottom'
                    }
                }
            }
        });
    }

    // Sales Chart (Food Admin)
    const salesCtx = document.getElementById('salesChart');
    if (salesCtx) {
        new Chart(salesCtx, {
            type: 'bar',
            data: {
                labels: ['Week 1', 'Week 2', 'Week 3', 'Week 4'],
                datasets: [{
                    label: 'Doanh thu (triệu đồng)',
                    data: [12, 19, 15, 22],
                    backgroundColor: '#e74c3c'
                }]
            },
            options: {
                responsive: true,
                plugins: {
                    legend: {
                        display: false
                    }
                },
                scales: {
                    y: {
                        beginAtZero: true
                    }
                }
            }
        });
    }
}

// Utility functions for admin
function confirmAction(message, callback) {
    if (confirm(message)) {
        callback();
    }
}

function showAdminNotification(message, type = 'info') {
    // Reuse the existing notification system
    showNotification(message, type);
}

// Export functions
window.adminFunctions = {
    toggleSidebar,
    toggleMobileSidebar,
    confirmAction,
    showAdminNotification
};

// Toggle password visibility
function togglePassword() {
    const passwordInput = document.getElementById('password');
    const toggleIcon = document.querySelector('.password-toggle i');

    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        toggleIcon.classList.remove('fa-eye');
        toggleIcon.classList.add('fa-eye-slash');
    } else {
        passwordInput.type = 'password';
        toggleIcon.classList.remove('fa-eye-slash');
        toggleIcon.classList.add('fa-eye');
    }
}

// Fill demo account info
function fillDemo(username, password) {
    document.getElementById('username').value = username;
    document.getElementById('password').value = password;

    // Add visual feedback
    const clickedAccount = event.currentTarget;
    clickedAccount.style.transform = 'scale(0.95)';
    setTimeout(() => {
        clickedAccount.style.transform = '';
    }, 150);
}

// Auto-submit demo accounts (optional)
function fillDemoAndSubmit(username, password) {
    fillDemo(username, password);

    // Auto submit after short delay
    setTimeout(() => {
        document.querySelector('.login-form').submit();
    }, 300);
}

//===========================================================================================================

// Update quantity
//function updateQuantity(itemId, change) {
//    const item = cartData.items.find(i => i.id === itemId);
//    if (item) {
//        const newQuantity = item.quantity + change;
//        if (newQuantity >= 1 && newQuantity <= 10) {
//            item.quantity = newQuantity;
//            updateCartDisplay();
//            updateCartSummary();
//        }

//    }
//}

// Update quantity from input
//function updateQuantityInput(itemId, value) {
//    const quantity = parseInt(value);
//    if (quantity >= 1 && quantity <= 10) {
//        const item = cartData.items.find(i => i.id === itemId);
//        if (item) {
//            item.quantity = quantity;
//            updateCartDisplay();
//            updateCartSummary();
//        }

//    }
//}

// Remove item from cart
//function removeItem(itemId) {
//    if (confirm('Bạn có chắc muốn xóa món này khỏi giỏ hàng?')) {
//        cartData.items = cartData.items.filter(i => i.id !== itemId);
//        if (cartData.items.length === 0) {
//            showEmptyCart();
//        }

//        else {
//            updateCartDisplay();
//            updateCartSummary();
//        }

//        showNotification('Đã xóa món khỏi giỏ hàng', 'success');
//    }
//}

// Clear entire cart
//function clearCart() {
//    if (confirm('Bạn có chắc muốn xóa tất cả món trong giỏ hàng?')) {
//        cartData.items = [];
//        showEmptyCart();
//        showNotification('Đã xóa tất cả món khỏi giỏ hàng', 'success');
//    }

//}

// Show empty cart state
//function showEmptyCart() {
//    document.querySelector('.cart-content').style.display = 'none';
//    document.getElementById('emptyCart').style.display = 'block';
//    updateCartCount(-999);
//    // Reset cart count
//}

// Update cart display
//function updateCartDisplay() {
//    const cartItems = document.querySelectorAll('.cart-item');
//    cartItems.forEach(item => {
//        const itemId = parseInt(item.dataset.id);
//        const cartItem = cartData.items.find(i => i.id === itemId);

//        if (cartItem) {
//            const quantityInput = item.querySelector('.quantity');
//            const totalPrice = item.querySelector('.total-price');

//            quantityInput.value = cartItem.quantity;
//            totalPrice.textContent = formatPrice(cartItem.price * cartItem.quantity);
//        }
//    });
//}

// Update cart summary
//function updateCartSummary() {
//    const subtotal = cartData.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
//    const total = subtotal + cartData.shippingFee - cartData.discount;
//    const totalItems = cartData.items.reduce((sum, item) => sum + item.quantity, 0);
//    document.getElementById('subtotal').textContent = formatPrice(subtotal);
//    document.getElementById('totalAmount').textContent = formatPrice(total);
//    document.querySelector('.cart-header h2').textContent = `Danh sách món ăn (${totalItems} món)`;
//    document.querySelector('.checkout-amount').textContent = formatPrice(total);
//    // Update navigation cart count updateCartCount(0, totalItems);
//}

// Apply coupon
//function applyCoupon() {
//    const couponCode = document.getElementById('couponCode').value.trim();
//    if (!couponCode) {
//        showNotification('Vui lòng nhập mã giảm giá', 'warning');
//        return;
//    }

//    // Simulate coupon validation
//    const validCoupons = {
//        'WELCOME10': 0.1, 'SAVE20': 0.2, 'NEWUSER': 27100 // Fixed amount
//    }

//        ;

    //if (validCoupons[couponCode]) {
    //    const subtotal = cartData.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
    //    if (typeof validCoupons[couponCode] === 'number' && validCoupons[couponCode] < 1) {
    //        // Percentage discount cartData.discount = subtotal * validCoupons[couponCode];
    //    }

    //    else {
    //        // Fixed amount discount cartData.discount = validCoupons[couponCode];
    //    }

    //    document.getElementById('discountRow').style.display = 'flex';
    //    document.getElementById('discountAmount').textContent = '-' + formatPrice(cartData.discount);

    //    updateCartSummary();
    //    showNotification('Đã áp dụng mã giảm giá thành công!', 'success');

    //    // Disable coupon input
    //    document.getElementById('couponCode').disabled = true;
    //    document.querySelector('.btn-apply-coupon').disabled = true;
    //    document.querySelector('.btn-apply-coupon').textContent = 'Đã áp dụng';
    //}

//    else {
//        showNotification('Mã giảm giá không hợp lệ', 'error');
//    }

//}

// Update delivery fee
//function updateDeliveryFee() {
//    const selectedDelivery = document.querySelector('input[name="delivery"]:checked').value;
//    const fees =

//    {
//        'standard': 15000, 'express': 25000, 'pickup': 0
//    }

//        ;

//    cartData.shippingFee = fees[selectedDelivery];
//    document.getElementById('shippingFee').textContent = formatPrice(cartData.shippingFee);
//    updateCartSummary();
//}

// Proceed to checkout
//function proceedToCheckout() {
//    if (cartData.items.length === 0) {
//        showNotification('Giỏ hàng của bạn đang trống', 'warning');
//        return;
//    }

//    // Save cart data to sessionStorage for checkout page
//    sessionStorage.setItem('cartData', JSON.stringify(cartData));

//    // Redirect to checkout
//    window.location.href = '/checkout';
//}

// Checkout page functions
function goBackToCart() {
    window.location.href = '/cart';
}

// Delivery time toggle
//document.addEventListener('DOMContentLoaded', function () {
//    // Toggle schedule options
//    const deliveryTimeRadios = document.querySelectorAll('input[name="deliveryTime"]');
//    const scheduleOptions = document.getElementById('scheduleOptions');

//    if (deliveryTimeRadios.length > 0) {
//        deliveryTimeRadios.forEach(radio => {
//            radio.addEventListener('change', function () {
//                if (this.value === 'schedule') {
//                    scheduleOptions.style.display = 'block';
//                    // Set minimum date to today
//                    const today = new Date().toISOString().split('T')[0];
//                    document.getElementById('deliveryDate').min = today;
//                    document.getElementById('deliveryDate').value = today;
//                } else {
//                    scheduleOptions.style.display = 'none';
//                }
//            });
//        });
//    }

//    // Handle checkout form submission
//    const checkoutForm = document.getElementById('checkoutForm');
//    if (checkoutForm) {
//        checkoutForm.addEventListener('submit', function (e) {
//            e.preventDefault();
//            handleCheckoutSubmission();
//        });
//    }

//    // Initialize cart summary on page load
//    if (window.location.pathname.includes('/Cart')) {
//        updateCartSummary();
//    }
//});

// Handle checkout form submission
//function handleCheckoutSubmission() {
//    // TRÁNH XUNG ĐỘT: Kiểm tra trang hiện tại
//    if (window.location.pathname.includes(`/${window.TABLE_CODE}/cart/checkout`)) {
//        return true; // Để trang checkout tự xử lý
//    }

//    // SỬA LỖI SYNTAX: Tách dòng đúng cách
//    const requiredFields = ['fullName', 'phone', 'address', 'district', 'ward'];
//    let isValid = true;

//    requiredFields.forEach(fieldName => {
//        const field = document.getElementById(fieldName);
//        if (field && !field.value.trim()) {
//            field.style.borderColor = '#dc3545';
//            isValid = false;
//        } else if (field) {
//            field.style.borderColor = '#ddd';
//        }
//    });

//    if (!isValid) {
//        showNotification('Vui lòng điền đầy đủ thông tin bắt buộc', 'error');
//        return false;
//    }

//    // Get form data
//    const formData = new FormData(document.getElementById('checkoutForm'));
//    const orderData = Object.fromEntries(formData);

//    // Add cart data - KIỂM TRA CARTDATA TỒN TẠI
//    if (typeof cartData !== 'undefined') {
//        orderData.items = cartData.items;
//        orderData.subtotal = cartData.items.reduce((sum, item) => sum + (item.price * item.quantity), 0);
//        orderData.shippingFee = cartData.shippingFee;
//        orderData.discount = cartData.discount;
//        orderData.total = orderData.subtotal + cartData.shippingFee - cartData.discount;
//    }

//    // Show loading state
//    const submitBtn = document.querySelector('.btn-place-order');
//    if (submitBtn) {
//        const originalText = submitBtn.innerHTML;
//        submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang xử lý...';
//        submitBtn.disabled = true;

//        // Simulate order processing
//        setTimeout(() => {
//            console.log('Order data:', orderData);
//            sessionStorage.removeItem('cartData');
//            // Ưu tiên helper đã phát từ Razor
//            window.location.href = (window.ROUTES && window.ROUTES.checkoutSuccess)
//                ? window.ROUTES.checkoutSuccess
//                : `/${window.TABLE_CODE}/cart/success`;
//        }, 2000);
//    }

//    return true;
//}

//document.addEventListener('DOMContentLoaded', function () {
//    // CHỈ BIND KHI KHÔNG PHẢI TRANG CHECKOUT
//    if (!window.location.pathname.includes(`/${window.TABLE_CODE}/cart/checkout`)) {
//        const checkoutForm = document.getElementById('checkoutForm');
//        if (checkoutForm) {
//            checkoutForm.addEventListener('submit', function (e) {
//                e.preventDefault();
//                handleCheckoutSubmission();
//            });
//        }
//    }

//    // Update cart count in navigation (enhanced version)
//    function updateCartCount(change = 0, newTotal = null) {
//        const cartCount = document.querySelector('.cart-count');
//        if (cartCount) {
//            if (newTotal !== null) {
//                cartCount.textContent = newTotal;
//            }

//            else {
//                let currentCount = parseInt(cartCount.textContent) || 0;
//                cartCount.textContent = Math.max(0, currentCount + change);
//            }

//            // Animation effect
//            cartCount.style.animation = 'bounce 0.5s ease';
//            setTimeout(() => {
//                cartCount.style.animation = '';
//            }, 500);
//        }
//    }

//    // Format price helper
//    function formatPrice(price) {
//        return new Intl.NumberFormat('vi-VN').format(price) + ' VNĐ';
//    }

//    // Enhanced notification system (sửa lại)
//    function showNotification(message, type = 'info') {
//        // Remove existing notifications first
//        const existingNotifications = document.querySelectorAll('.notification');
//        existingNotifications.forEach(notif => notif.remove());

//        const notification = document.createElement('div');
//        notification.className = `notification notification-${type}`;

//        const icons = {
//            'success': 'fas fa-check-circle',
//            'error': 'fas fa-exclamation-circle',
//            'warning': 'fas fa-exclamation-triangle',
//            'info': 'fas fa-info-circle'
//        };

//        notification.innerHTML = `
//        <div class="notification-content">
//            <i class="${icons[type]} notification-icon"></i>
//            <span class="notification-message">${message}</span>
//            <button class="notification-close" onclick="closeNotification(this)">
//                <i class="fas fa-times"></i>
//            </button>
//        </div>
//    `;

//        document.body.appendChild(notification);

//        // Add show class for animation
//        setTimeout(() => {
//            notification.classList.add('notification-show');
//        }, 10);

//        // Auto remove after 4 seconds
//        setTimeout(() => {
//            closeNotification(notification.querySelector('.notification-close'));
//        }, 4000);
//    }

//    function closeNotification(closeBtn) {
//        const notification = closeBtn.closest('.notification');
//        if (notification) {
//            notification.classList.add('notification-hide');
//            setTimeout(() => {
//                if (notification.parentElement) {
//                    notification.remove();
//                }
//            }, 300);
//        }
//    }

//    // News filter functionality
//    document.addEventListener('DOMContentLoaded', function () {
//        // News filter buttons
//        const filterButtons = document.querySelectorAll('.filter-btn');
//        const newsItems = document.querySelectorAll('.news-item');

//        filterButtons.forEach(button => {
//            button.addEventListener('click', function () {
//                const category = this.getAttribute('data-category');

//                // Update active button
//                filterButtons.forEach(btn => btn.classList.remove('active'));
//                this.classList.add('active');

//                // Filter news items
//                newsItems.forEach(item => {
//                    if (category === 'all' || item.getAttribute('data-category') === category) {
//                        item.style.display = 'block';
//                    } else {
//                        item.style.display = 'none';
//                    }
//                });
//            });
//        });

//        // Load more news
//        const loadMoreBtn = document.querySelector('.btn-load-more');
//        if (loadMoreBtn) {
//            loadMoreBtn.addEventListener('click', function () {
//                showNotification('Tính năng tải thêm tin tức sẽ được cập nhật', 'info');
//            });
//        }
//    });

//    // FAQ toggle functionality
//    function toggleFAQ(element) {
//        const faqItem = element.parentElement;
//        const isActive = faqItem.classList.contains('active');

//        // Close all FAQ items
//        document.querySelectorAll('.faq-item').forEach(item => {
//            item.classList.remove('active');
//        });

//        // Toggle current item
//        if (!isActive) {
//            faqItem.classList.add('active');
//        }
//    }

//    // Contact form validation
//    document.addEventListener('DOMContentLoaded', function () {
//        const contactForm = document.querySelector('.contact-form');

//        if (contactForm) {
//            contactForm.addEventListener('submit', function (e) {
//                e.preventDefault();

//                // Get form data
//                const name = document.getElementById('name').value.trim();
//                const email = document.getElementById('email').value.trim();
//                const subject = document.getElementById('subject').value;
//                const message = document.getElementById('message').value.trim();

//                // Basic validation
//                if (!name || !email || !subject || !message) {
//                    showNotification('Vui lòng điền đầy đủ thông tin bắt buộc', 'warning');
//                    return;
//                }

//                if (!isValidEmail(email)) {
//                    showNotification('Email không hợp lệ', 'error');
//                    return;
//                }

//                // Show loading
//                const submitBtn = this.querySelector('.btn-submit');
//                const originalText = submitBtn.innerHTML;
//                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin"></i> Đang gửi...';
//                submitBtn.disabled = true;

//                // Simulate form submission
//                setTimeout(() => {
//                    showNotification('Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi trong 24h.', 'success');
//                    contactForm.reset();
//                    submitBtn.innerHTML = originalText;
//                    submitBtn.disabled = false;
//                }, 2000);
//            });
//        }
//    });

//    // Email validation helper
//    function isValidEmail(email) {
//        const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
//        return emailRegex.test(email);
//    }

//});
