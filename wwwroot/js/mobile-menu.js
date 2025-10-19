// ===== MOBILE MENU FUNCTIONALITY =====
document.addEventListener('DOMContentLoaded', function () {
    const mobileToggle = document.getElementById('mobileMenuToggle');
    const mobileMenu = document.getElementById('mobileMenu');
    const dropdownToggles = document.querySelectorAll('.mobile-dropdown-toggle');

    // Mobile menu toggle
    if (mobileToggle && mobileMenu) {
        mobileToggle.addEventListener('click', function (e) {
            e.preventDefault();
            e.stopPropagation();

            mobileToggle.classList.toggle('active');
            mobileMenu.classList.toggle('show');

            // Prevent body scroll when menu is open
            if (mobileMenu.classList.contains('show')) {
                document.body.style.overflow = 'hidden';
            } else {
                document.body.style.overflow = '';
            }
        });

        // Close menu when clicking overlay
        mobileMenu.addEventListener('click', function (e) {
            if (e.target === mobileMenu) {
                closeMobileMenu();
            }
        });
    }

    // Mobile dropdown toggles
    dropdownToggles.forEach(toggle => {
        toggle.addEventListener('click', function () {
            const dropdownMenu = this.nextElementSibling;
            const isOpen = dropdownMenu.classList.contains('show');

            // Close all other dropdowns
            dropdownToggles.forEach(otherToggle => {
                if (otherToggle !== this) {
                    otherToggle.classList.remove('active');
                    otherToggle.nextElementSibling.classList.remove('show');
                }
            });

            // Toggle current dropdown
            this.classList.toggle('active');
            dropdownMenu.classList.toggle('show');
        });
    });

    // Close menu when clicking on links
    const mobileNavLinks = document.querySelectorAll('.mobile-nav-link, .mobile-dropdown-menu a');
    mobileNavLinks.forEach(link => {
        link.addEventListener('click', function () {
            setTimeout(closeMobileMenu, 100);
        });
    });

    function closeMobileMenu() {
        mobileToggle.classList.remove('active');
        mobileMenu.classList.remove('show');
        document.body.style.overflow = '';

        // Close all dropdowns
        dropdownToggles.forEach(toggle => {
            toggle.classList.remove('active');
            toggle.nextElementSibling.classList.remove('show');
        });
    }

    // Handle window resize
    window.addEventListener('resize', function () {
        if (window.innerWidth > 768 && mobileMenu.classList.contains('show')) {
            closeMobileMenu();
        }
    });

    // Close menu with ESC key
    document.addEventListener('keydown', function (e) {
        if (e.key === 'Escape' && mobileMenu.classList.contains('show')) {
            closeMobileMenu();
        }
    });
});

console.log('✅ Mobile menu JavaScript loaded successfully');
