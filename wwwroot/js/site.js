//// Sticky Navigation & Scroll Effects
//const PERSISTENT_NOTIFICATION_KEY = 'site:persistentNotifications';

//function loadPersistentNotifications() {
//    try {
//        const raw = sessionStorage.getItem(PERSISTENT_NOTIFICATION_KEY);
//        if (!raw) {
//            return [];
//        }

//        const parsed = JSON.parse(raw);
//        if (!Array.isArray(parsed)) {
//            return [];
//        }

//        const sanitized = parsed
//            .map(item => {
//                if (!item || typeof item !== 'object') {
//                    return null;
//                }

//                const id = typeof item.id === 'string' && item.id ? item.id : null;
//                const message = typeof item.message === 'string' ? item.message : '';
//                if (!id || !message) {
//                    return null;
//                }

//                const type = typeof item.type === 'string' && item.type ? item.type : 'info';
//                const duration = Number.isFinite(item.duration) && item.duration > 0
//                    ? item.duration
//                    : 3000;

//                return {
//                    id,
//                    message,
//                    type,
//                    duration
//                };
//            })
//            .filter(Boolean);

//        return sanitized;
//    } catch {
//        return [];
//    }
//}

//function savePersistentNotifications(entries) {
//    try {
//        if (!Array.isArray(entries) || !entries.length) {
//            sessionStorage.removeItem(PERSISTENT_NOTIFICATION_KEY);
//            return;
//        }

//        sessionStorage.setItem(PERSISTENT_NOTIFICATION_KEY, JSON.stringify(entries));
//    } catch {
//        // ignore storage errors (e.g. private mode)
//    }
//}

//function persistNotificationEntry(entry) {
//    const queue = loadPersistentNotifications();
//    const nextQueue = queue.filter(item => item.id !== entry.id);
//    nextQueue.push(entry);
//    savePersistentNotifications(nextQueue);
//}

//function removePersistentNotification(id) {
//    if (!id) {
//        return;
//    }

//    const queue = loadPersistentNotifications();
//    const nextQueue = queue.filter(item => item.id !== id);
//    savePersistentNotifications(nextQueue);
//}

//document.addEventListener('DOMContentLoaded', function () {
//    const header = document.querySelector('.header-nav');
//    const mobileToggle = document.querySelector('.mobile-menu-toggle');
//    const navMenu = document.querySelector('.nav-menu');

//    if (header) {
//        // Sticky navigation với hiệu ứng
//        window.addEventListener('scroll', function () {
//            if (window.scrollY > 100) {
//                header.classList.add('scrolled');
//            } else {
//                header.classList.remove('scrolled');
//            }
//        });
//    }

//    // Mobile menu toggle
//    if (mobileToggle && navMenu) {
//        mobileToggle.addEventListener('click', function () {
//            navMenu.classList.toggle('mobile-active');
//            this.classList.toggle('active');
//        });
//    }

//    // Smooth scroll cho internal links
//    document.querySelectorAll('a[href^="#"]').forEach(anchor => {
//        anchor.addEventListener('click', function (e) {
//            e.preventDefault();
//            const target = document.querySelector(this.getAttribute('href'));
//            if (target) {
//                target.scrollIntoView({
//                    behavior: 'smooth',
//                    block: 'start'
//                });
//            }
//        });
//    });

//    // Animate elements on scroll
//    const observerOptions = {
//        threshold: 0.1,
//        rootMargin: '0px 0px -50px 0px'
//    };

//    const observer = new IntersectionObserver(function (entries) {
//        entries.forEach(entry => {
//            if (entry.isIntersecting) {
//                entry.target.classList.add('fade-in-up');
//            }
//        });
//    }, observerOptions);

//    // Observe elements for animation
//    document.querySelectorAll('.menu-item-card, .footer-section').forEach(el => {
//        observer.observe(el);
//    });

//    const queuedNotifications = loadPersistentNotifications();
//    if (queuedNotifications.length) {
//        // ensure sanitized queue is written back so invalid entries are cleared
//        savePersistentNotifications(queuedNotifications);
//        queuedNotifications.forEach(entry => {
//            removePersistentNotification(entry.id);
//            showNotification(entry.message, entry.type, {
//                duration: entry.duration,
//                storageId: entry.id
//            });
//        });
//    }
//});

//function addToCart(foodId) {
//    // Hiệu ứng thêm vào giỏ hàng
//    const cartBtn = document.querySelector('.cart-btn');
//    if (cartBtn) {
//        cartBtn.style.transform = 'scale(1.1)';
//        cartBtn.style.background = 'var(--success-color)';

//        setTimeout(() => {
//            cartBtn.style.transform = '';
//            cartBtn.style.background = '';
//        }, 300);
//    }

//    showNotification('Đã thêm món vào giỏ hàng!', 'success');
//}

//function showNotification(message, type = 'info', options) {
//    const opts = (options && typeof options === 'object') ? options : {};
//    const storageId = typeof opts.storageId === 'string' && opts.storageId ? opts.storageId : null;
//    const persist = opts.persist === true;
//    const duration = Number.isFinite(opts.duration) && opts.duration > 0 ? opts.duration : 5000;
//    const normalizedType = typeof type === 'string' ? type.toLowerCase() : 'info';
//    const resolvedType = ['success', 'error', 'warning', 'info'].includes(normalizedType)
//        ? normalizedType
//        : (normalizedType === 'danger' ? 'error' : 'info');

//    const id = storageId || `notif-${Date.now()}-${Math.random().toString(36).slice(2, 10)}`;

//    if (persist && !storageId) {
//        persistNotificationEntry({
//            id,
//            message,
//            type: resolvedType,
//            duration
//        });
//    }

//    let container = document.querySelector('.notification-container');
//    if (!container) {
//        container = document.createElement('div');
//        container.className = 'notification-container';
//        container.setAttribute('role', 'region');
//        container.setAttribute('aria-live', 'polite');
//        document.body.appendChild(container);
//    }

//    const existing = container.querySelector(`.notification[data-notification-id="${id}"]`);
//    if (existing) {
//        existing.remove();
//    }

//    const notification = document.createElement('div');
//    notification.className = `notification notification-${resolvedType}`;
//    notification.dataset.notificationId = id;
//    notification.dataset.notificationType = resolvedType;
//    notification.setAttribute('role', 'status');

//    const content = document.createElement('div');
//    content.className = 'notification-content';

//    const icon = document.createElement('i');
//    const iconClass = {
//        success: 'fa-check-circle',
//        warning: 'fa-triangle-exclamation',
//        error: 'fa-circle-xmark'
//    }[resolvedType] || 'fa-circle-info';
//    icon.className = `fas notification-icon ${iconClass}`;

//    const messageSpan = document.createElement('span');
//    messageSpan.className = 'notification-message';
//    messageSpan.textContent = message;

//    const closeButton = document.createElement('button');
//    closeButton.type = 'button';
//    closeButton.className = 'notification-close';
//    closeButton.setAttribute('aria-label', 'Đóng thông báo');
//    closeButton.innerHTML = '<i class="fas fa-times"></i>';

//    content.appendChild(icon);
//    content.appendChild(messageSpan);
//    content.appendChild(closeButton);
//    notification.appendChild(content);
//    container.appendChild(notification);

//    window.requestAnimationFrame(() => {
//        notification.classList.add('notification-show');
//    });

//    let dismissed = false;
//    let hideTimer = null;

//    const finalizeRemoval = () => {
//        if (dismissed) {
//            return;
//        }
//        dismissed = true;
//        if (persist || storageId) {
//            removePersistentNotification(id);
//        }
//        notification.remove();
//        if (!container.childElementCount) {
//            container.remove();
//        }
//    };

//    const hideNotification = () => {
//        if (dismissed) {
//            return;
//        }
//        dismissed = true;
//        notification.classList.remove('notification-show');
//        notification.classList.add('notification-hide');
//        window.setTimeout(finalizeRemoval, 250);
//    };

//    const shouldAutoHide = opts.autoHide !== false;

//    const startHideTimer = (delay) => {
//        if (shouldAutoHide && delay > 0) {
//            hideTimer = window.setTimeout(hideNotification, delay);
//        }
//    };

//    const clearHideTimer = () => {
//        if (hideTimer) {
//            window.clearTimeout(hideTimer);
//            hideTimer = null;
//        }
//    };

//    if (shouldAutoHide) {
//        startHideTimer(duration);
//    }

//    notification.addEventListener('mouseenter', clearHideTimer);
//    notification.addEventListener('mouseleave', () => {
//        if (!dismissed && !persist) {
//            startHideTimer(1500);
//        }
//    });

//    closeButton.addEventListener('click', (event) => {
//        event.preventDefault();
//        event.stopPropagation();
//        clearHideTimer();
//        hideNotification();
//    });

//    notification.addEventListener('click', (event) => {
//        if (event.target === closeButton || closeButton.contains(event.target)) {
//            return;
//        }
//        clearHideTimer();
//        hideNotification();
//    });

//    return {
//        id,
//        element: notification,
//        remove: finalizeRemoval
//    };
//}

//function updateCartCount(addQuantity = 1) {
//    const cartCount = document.querySelector('.cart-count');
//    if (cartCount) {
//        let currentCount = parseInt(cartCount.textContent) || 0;
//        cartCount.textContent = currentCount + addQuantity;

//        // Animation effect
//        cartCount.style.animation = 'bounce 0.5s ease';
//        setTimeout(() => {
//            cartCount.style.animation = '';
//        }, 500);
//    }
//}

//// Admin Panel JavaScript Functions

//// Sidebar toggle
//function toggleSidebar() {
//    const sidebar = document.querySelector('.admin-sidebar');

//    if (!sidebar) {
//        return;
//    }

//    sidebar.classList.toggle('collapsed');

//    const themeIcon = document.querySelector('.theme-toggle .theme-icon');
//    const themeText = document.querySelector('.theme-toggle .theme-text');
//    if (themeIcon) {
//        const isDark = document.body.classList.contains('dark-theme');
//        const isCollapsed = sidebar.classList.contains('collapsed');
//        themeIcon.textContent = isCollapsed ? (isDark ? 'light_mode' : 'dark_mode') : 'dark_mode';
//        if (themeText) {
//            themeText.textContent = isDark ? 'Chế độ sáng' : 'Chế độ tối';
//        }
//    }
//}

//// Mobile sidebar toggle
//function toggleMobileSidebar() {
//    const sidebar = document.querySelector('.admin-sidebar');

//    if (!sidebar) {
//        return;
//    }

//    sidebar.classList.toggle('collapsed');

//    const themeIcon = document.querySelector('.theme-toggle .theme-icon');
//    const themeText = document.querySelector('.theme-toggle .theme-text');
//    if (themeIcon) {
//        const isDark = document.body.classList.contains('dark-theme');
//        const isCollapsed = sidebar.classList.contains('collapsed');
//        themeIcon.textContent = isCollapsed ? (isDark ? 'light_mode' : 'dark_mode') : 'dark_mode';
//        if (themeText) {
//            themeText.textContent = isDark ? 'Chế độ sáng' : 'Chế độ tối';
//        }
//    }
//}

//// Dashboard functions
//document.addEventListener('DOMContentLoaded', function () {
//    const currentPath = window.location.pathname;
//    const navItems = document.querySelectorAll('.nav-item');

//    navItems.forEach(item => {
//        const href = item.getAttribute('href');
//        if (!href) {
//            return;
//        }

//        const linkPath = new URL(href, window.location.origin).pathname;
//        if (currentPath === linkPath) {
//            item.classList.add('active');
//        }
//    });
//});

//// Utility functions for admin
//function confirmAction(message, callback) {
//    if (confirm(message)) {
//        callback();
//    }
//}

//function showAdminNotification(message, type = 'info') {
//    // Reuse the existing notification system
//    showNotification(message, type);
//}

//// Export functions
//window.adminFunctions = {
//    toggleSidebar,
//    toggleMobileSidebar,
//    confirmAction,
//    showAdminNotification
//};
