//// ===== MENU PAGE JAVASCRIPT - FIXED VERSION ===== 
//(function () {
//    'use strict';

//    // Wait for DOM to be fully loaded
//    if (document.readyState === 'loading') {
//        document.addEventListener('DOMContentLoaded', initMenuPage);
//    } else {
//        initMenuPage();
//    }

//    function initMenuPage() {
//        // Check if we're on the menu page
//        const menuGrid = document.getElementById('menuGrid');
//        if (!menuGrid) {
//            console.log('Menu page not detected, skipping initialization');
//            return;
//        }

//        console.log('Initializing menu page...');

//        const categoryBtns = document.querySelectorAll('.category-btn');
//        const menuItems = document.querySelectorAll('.menu-item-card');
//        const emptyState = document.getElementById('emptyState');

//        console.log(`Found ${categoryBtns.length} category buttons`);
//        console.log(`Found ${menuItems.length} menu items`);

//        // Category filter functionality
//        categoryBtns.forEach(btn => {
//            btn.addEventListener('click', function (e) {
//                e.preventDefault();
//                console.log('Category button clicked:', this.dataset.category);

//                // Prevent double clicks
//                if (this.classList.contains('active')) {
//                    console.log('Button already active, skipping');
//                    return;
//                }

//                // Update active state
//                categoryBtns.forEach(b => b.classList.remove('active'));
//                this.classList.add('active');

//                const category = this.dataset.category;
//                console.log('Filtering by category:', category);

//                // Add loading class
//                menuGrid.classList.add('filtering');

//                // Filter with animation
//                filterMenuItemsSmooth(category);

//                // Remove loading class
//                setTimeout(() => {
//                    menuGrid.classList.remove('filtering');
//                }, 400);
//            });
//        });

//        function filterMenuItemsSmooth(category) {
//            let visibleCount = 0;
//            console.log('Starting filter for category:', category);

//            // Filter items
//            menuItems.forEach((item, index) => {
//                const itemCategory = item.dataset.category;
//                console.log(`Item ${index}: category=${itemCategory}, target=${category}`);

//                if (category === 'all' || itemCategory === category) {
//                    // Show item with delay for stagger effect
//                    setTimeout(() => {
//                        item.style.display = 'block';
//                        item.classList.add('filtered-in');
//                        item.classList.remove('filtered-out');
//                        item.style.opacity = '1';
//                        item.style.transform = 'translateY(0)';
//                    }, index * 50);
//                    visibleCount++;
//                } else {
//                    // Hide item immediately
//                    item.classList.add('filtered-out');
//                    item.classList.remove('filtered-in');
//                    item.style.opacity = '0';
//                    item.style.transform = 'translateY(20px)';
//                    setTimeout(() => {
//                        item.style.display = 'none';
//                    }, 200);
//                }
//            });

//            console.log(`Visible items: ${visibleCount}`);

//            // Handle empty state
//            setTimeout(() => {
//                if (emptyState) {
//                    if (visibleCount === 0) {
//                        emptyState.style.display = 'block';
//                        emptyState.style.opacity = '1';
//                    } else {
//                        emptyState.style.display = 'none';
//                    }
//                }
//            }, 300);
//        }

//        // Initialize with current active category
//        const activeBtn = document.querySelector('.category-btn.active');
//        if (activeBtn) {
//            console.log('Found active button:', activeBtn.dataset.category);
//            filterMenuItemsSmooth(activeBtn.dataset.category);
//        } else {
//            console.log('No active button found, showing all items');
//            filterMenuItemsSmooth('all');
//        }

//        console.log('Menu page initialized successfully');
//    }

//    // Global functions
//    window.showAllItems = function () {
//        console.log('Show all items called');
//        const allBtn = document.querySelector('.category-btn[data-category="all"]');
//        if (allBtn) {
//            // Remove active from current button
//            document.querySelectorAll('.category-btn').forEach(b => b.classList.remove('active'));
//            // Add active to "Tất cả" button
//            allBtn.classList.add('active');
//            // Show all items
//            document.querySelectorAll('.menu-item-card').forEach(item => {
//                item.style.display = 'block';
//                item.style.opacity = '1';
//                item.style.transform = 'translateY(0)';
//                item.classList.add('filtered-in');
//                item.classList.remove('filtered-out');
//            });
//            // Hide empty state
//            const emptyState = document.getElementById('emptyState');
//            if (emptyState) {
//                emptyState.style.display = 'none';
//            }
//        }
//    };

//    // Add to cart with enhanced feedback
//    window.addToCart = function (itemName, price) {
//        console.log('Add to cart called:', itemName, price);

//        const button = event.target.closest('.btn-add-cart');
//        if (!button) {
//            console.log('Button not found');
//            return;
//        }

//        const originalHTML = button.innerHTML;

//        // Visual feedback
//        button.innerHTML = '<i class="fas fa-check"></i> Đã thêm';
//        button.style.background = 'linear-gradient(135deg, #28a745, #20c997)';
//        button.disabled = true;

//        // Reset button after 2 seconds
//        setTimeout(() => {
//            button.innerHTML = originalHTML;
//            button.style.background = '';
//            button.disabled = false;
//        }, 2000);

//        // Show notification
//        showNotification(`Đã thêm "${itemName}" vào giỏ hàng!`, 'success');

//        // Log for debugging
//        console.log('Added to cart:', { name: itemName, price: price });

//        // Add to localStorage (optional)
//        addToLocalStorage(itemName, price);
//    };

//    // Add to localStorage
//    function addToLocalStorage(itemName, price) {
//        try {
//            let cart = JSON.parse(localStorage.getItem('cart')) || [];

//            // Check if item already exists
//            const existingItem = cart.find(item => item.name === itemName);

//            if (existingItem) {
//                existingItem.quantity += 1;
//                existingItem.total = existingItem.quantity * existingItem.price;
//            } else {
//                cart.push({
//                    name: itemName,
//                    price: price,
//                    quantity: 1,
//                    total: price,
//                    addedAt: new Date().toISOString()
//                });
//            }

//            localStorage.setItem('cart', JSON.stringify(cart));
//            updateCartBadge();
//        } catch (error) {
//            console.error('Error saving to localStorage:', error);
//        }
//    }

//    // Update cart badge (if you have one in layout)
//    function updateCartBadge() {
//        try {
//            const cart = JSON.parse(localStorage.getItem('cart')) || [];
//            const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);

//            const badge = document.querySelector('.cart-badge');
//            if (badge) {
//                badge.textContent = totalItems;
//                badge.style.display = totalItems > 0 ? 'block' : 'none';
//            }
//        } catch (error) {
//            console.error('Error updating cart badge:', error);
//        }
//    }

//    // Enhanced notification system
//    function showNotification(message, type = 'info') {
//        console.log('Showing notification:', message, type);

//        // Remove existing notifications
//        document.querySelectorAll('.notification').forEach(n => n.remove());

//        const notification = document.createElement('div');
//        notification.className = `notification ${type}`;
//        notification.innerHTML = `
//            <i class="fas fa-${type === 'success' ? 'check' : 'info'}-circle"></i>
//            <span>${message}</span>
//            <button class="close-btn" onclick="this.parentElement.remove()">×</button>
//        `;

//        document.body.appendChild(notification);

//        // Show animation
//        setTimeout(() => {
//            notification.classList.add('show');
//        }, 100);

//        // Auto remove after 4 seconds
//        setTimeout(() => {
//            if (document.body.contains(notification)) {
//                notification.classList.remove('show');
//                setTimeout(() => {
//                    if (document.body.contains(notification)) {
//                        document.body.removeChild(notification);
//                    }
//                }, 400);
//            }
//        }, 4000);
//    }

//    // Add CSS animations dynamically (only once)
//    if (!document.querySelector('#menu-page-styles')) {
//        const menuPageStyles = document.createElement('style');
//        menuPageStyles.id = 'menu-page-styles';
//        menuPageStyles.textContent = `
//            .menu-item-card {
//                transition: all 0.4s ease;
//                transform-origin: center;
//            }
            
//            .menu-item-card.filtered-out {
//                opacity: 0;
//                transform: scale(0.8) translateY(20px);
//                pointer-events: none;
//            }

//            .menu-item-card.filtered-in {
//                opacity: 1;
//                transform: scale(1) translateY(0);
//                pointer-events: auto;
//            }
            
//            .filtering {
//                opacity: 0.7;
//                pointer-events: none;
//            }
            
//            @keyframes fadeInUp {
//                from {
//                    opacity: 0;
//                    transform: translateY(30px);
//                }
//                to {
//                    opacity: 1;
//                    transform: translateY(0);
//                }
//            }
            
//            @keyframes fadeIn {
//                from {
//                    opacity: 0;
//                }
//                to {
//                    opacity: 1;
//                }
//            }
            
//            @keyframes bounce {
//                0%, 20%, 53%, 80%, 100% {
//                    transform: translate3d(0,0,0);
//                }
//                40%, 43% {
//                    transform: translate3d(0, -5px, 0);
//                }
//                70% {
//                    transform: translate3d(0, -3px, 0);
//                }
//                90% {
//                    transform: translate3d(0, -1px, 0);
//                }
//            }
            
//            .btn-add-cart.added {
//                animation: bounce 0.6s ease;
//            }
//        `;
//        document.head.appendChild(menuPageStyles);
//    }

//    // Initialize cart badge on page load
//    updateCartBadge();

//})();

//console.log('✅ Menu page JavaScript loaded successfully');
