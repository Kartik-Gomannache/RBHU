/* ============================================
   MOBILE MENU FUNCTIONALITY
   Professional slide-down menu with smooth animations
   ============================================ */

(function () {
    'use strict';

    // Get DOM elements
    const mobileMenuToggle = document.getElementById('mobileMenuToggle');
    const mobileMenuClose = document.getElementById('mobileMenuClose');
    const mobileMenuOverlay = document.getElementById('mobileMenuOverlay');
    const navbarMenu = document.getElementById('navbarMenu');
    const body = document.body;
    const navLinks = document.querySelectorAll('.navbar-menu .nav-link');

    // Check if elements exist
    if (!mobileMenuToggle || !mobileMenuClose || !mobileMenuOverlay || !navbarMenu) {
        console.warn('Mobile menu elements not found');
        return;
    }

    /**
     * Open mobile menu
     */
    function openMobileMenu() {
        navbarMenu.classList.add('active');
        mobileMenuOverlay.classList.add('active');
        mobileMenuToggle.classList.add('active');
        body.classList.add('menu-open');

        // Disable scroll on body
        body.style.overflow = 'hidden';

        // Set aria attributes for accessibility
        mobileMenuToggle.setAttribute('aria-expanded', 'true');
        navbarMenu.setAttribute('aria-hidden', 'false');

        // Focus on close button for keyboard navigation
        setTimeout(() => {
            mobileMenuClose.focus();
        }, 100);
    }

    /**
     * Close mobile menu
     */
    function closeMobileMenu() {
        navbarMenu.classList.remove('active');
        mobileMenuOverlay.classList.remove('active');
        mobileMenuToggle.classList.remove('active');
        body.classList.remove('menu-open');

        // Re-enable scroll on body
        body.style.overflow = '';

        // Set aria attributes for accessibility
        mobileMenuToggle.setAttribute('aria-expanded', 'false');
        navbarMenu.setAttribute('aria-hidden', 'true');

        // Return focus to toggle button
        setTimeout(() => {
            mobileMenuToggle.focus();
        }, 100);
    }

    /**
     * Toggle mobile menu
     */
    function toggleMobileMenu() {
        if (navbarMenu.classList.contains('active')) {
            closeMobileMenu();
        } else {
            openMobileMenu();
        }
    }

    // Event Listeners

    // Toggle button click
    mobileMenuToggle.addEventListener('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        toggleMobileMenu();
    });

    // Close button click
    mobileMenuClose.addEventListener('click', function (e) {
        e.preventDefault();
        e.stopPropagation();
        closeMobileMenu();
    });

    // Overlay click
    mobileMenuOverlay.addEventListener('click', function (e) {
        e.preventDefault();
        closeMobileMenu();
    });

    // Close menu when nav link is clicked
    navLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            // Don't close if it's the products dropdown toggle
            if (!this.classList.contains('products-dropdown-toggle')) {
                closeMobileMenu();
            }
        });
    });

    // Close menu on window resize if open (when viewport becomes desktop size)
    let resizeTimer;
    window.addEventListener('resize', function () {
        clearTimeout(resizeTimer);
        resizeTimer = setTimeout(function () {
            if (window.innerWidth > 991 && navbarMenu.classList.contains('active')) {
                closeMobileMenu();
            }
        }, 250);
    });

    // Keyboard navigation
    document.addEventListener('keydown', function (e) {
        // ESC key closes menu
        if (e.key === 'Escape' && navbarMenu.classList.contains('active')) {
            closeMobileMenu();
        }

        // Tab key trap (keep focus inside menu when open)
        if (e.key === 'Tab' && navbarMenu.classList.contains('active')) {
            const focusableElements = navbarMenu.querySelectorAll(
                'a[href], button:not([disabled]), textarea, input, select'
            );
            const firstElement = mobileMenuClose;
            const lastElement = focusableElements[focusableElements.length - 1];

            // Shift + Tab (moving backwards)
            if (e.shiftKey) {
                if (document.activeElement === firstElement) {
                    e.preventDefault();
                    lastElement.focus();
                }
            }
            // Tab (moving forwards)
            else {
                if (document.activeElement === lastElement) {
                    e.preventDefault();
                    firstElement.focus();
                }
            }
        }
    });

    // Prevent body scroll when menu is open (iOS fix)
    let scrollY = 0;

    function preventBodyScroll() {
        scrollY = window.scrollY;
        body.style.position = 'fixed';
        body.style.top = `-${scrollY}px`;
        body.style.width = '100%';
    }

    function enableBodyScroll() {
        body.style.position = '';
        body.style.top = '';
        body.style.width = '';
        window.scrollTo(0, scrollY);
    }

    // Update open/close functions to use scroll prevention
    const originalOpen = openMobileMenu;
    openMobileMenu = function () {
        originalOpen();
        if (window.innerWidth <= 991) {
            preventBodyScroll();
        }
    };

    const originalClose = closeMobileMenu;
    closeMobileMenu = function () {
        originalClose();
        if (window.innerWidth <= 991) {
            enableBodyScroll();
        }
    };

    // Initialize aria attributes
    mobileMenuToggle.setAttribute('aria-expanded', 'false');
    mobileMenuToggle.setAttribute('aria-controls', 'navbarMenu');
    mobileMenuToggle.setAttribute('aria-label', 'Open navigation menu');
    navbarMenu.setAttribute('aria-hidden', 'true');
    mobileMenuClose.setAttribute('aria-label', 'Close navigation menu');

    // Smooth scroll to top when clicking logo (optional enhancement)
    const navbarBrand = document.querySelector('.navbar-brand');
    if (navbarBrand) {
        navbarBrand.addEventListener('click', function (e) {
            // Only if on same page (href matches current page)
            const href = this.getAttribute('href');
            if (href && (href === window.location.pathname || href.includes('#'))) {
                window.scrollTo({ top: 0, behavior: 'smooth' });
            }
        });
    }

    // Close menu on orientation change (mobile devices)
    window.addEventListener('orientationchange', function () {
        if (navbarMenu.classList.contains('active')) {
            setTimeout(closeMobileMenu, 100);
        }
    });

    // Debugging info (remove in production)
    console.log('Mobile menu initialized successfully');

    // Export functions for external use (if needed)
    window.MobileMenu = {
        open: openMobileMenu,
        close: closeMobileMenu,
        toggle: toggleMobileMenu,
        isOpen: function () {
            return navbarMenu.classList.contains('active');
        }
    };

})();

/* ============================================
   ADDITIONAL ENHANCEMENTS
   ============================================ */

// Add touch swipe functionality to close menu (swipe up)
(function () {
    const navbarMenu = document.getElementById('navbarMenu');
    if (!navbarMenu) return;

    let touchStartY = 0;
    let touchEndY = 0;

    navbarMenu.addEventListener('touchstart', function (e) {
        touchStartY = e.changedTouches[0].screenY;
    }, { passive: true });

    navbarMenu.addEventListener('touchend', function (e) {
        touchEndY = e.changedTouches[0].screenY;
        handleSwipe();
    }, { passive: true });

    function handleSwipe() {
        const swipeDistance = touchEndY - touchStartY;
        const minSwipeDistance = 50;

        // Swipe up to close
        if (swipeDistance < -minSwipeDistance) {
            if (window.MobileMenu && window.MobileMenu.isOpen()) {
                window.MobileMenu.close();
            }
        }
    }
})();

// Add active state to current page nav link
(function () {
    const currentPath = window.location.pathname;
    const navLinks = document.querySelectorAll('.navbar-menu .nav-link');

    navLinks.forEach(link => {
        const href = link.getAttribute('href');
        if (href && (href === currentPath || currentPath.includes(href))) {
            link.classList.add('active');
            link.style.borderLeftColor = 'var(--accent-color)';
            link.style.background = 'rgba(255, 255, 255, 0.1)';
        }
    });
})();
