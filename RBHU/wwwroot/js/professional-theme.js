/* ==========================================
   PROFESSIONAL THEME MANAGER
   Clean, Minimal Design System
   ========================================== */

(function () {
    'use strict';

    const STORAGE_KEY = 'appTheme';
    const THEME_LIGHT = 'light';
    const THEME_DARK = 'dark';

    // Initialize theme on load
    function initTheme() {
        const savedTheme = getSavedTheme();
        applyTheme(savedTheme);

        // Listen for storage changes from other tabs
        window.addEventListener('storage', onStorageChange);
    }

    // Get saved theme
    function getSavedTheme() {
        const saved = localStorage.getItem(STORAGE_KEY);
        if (saved && (saved === THEME_DARK || saved === THEME_LIGHT)) {
            return saved;
        }

        // Check system preference
        if (window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches) {
            return THEME_DARK;
        }

        return THEME_LIGHT;
    }

    // Apply theme
    function applyTheme(theme) {
        if (theme !== THEME_DARK && theme !== THEME_LIGHT) {
            theme = THEME_LIGHT;
        }

        document.documentElement.setAttribute('data-theme', theme);
        localStorage.setItem(STORAGE_KEY, theme);

        updateAllThemeButtons(theme);
        window.dispatchEvent(new CustomEvent('themechange', { detail: { theme: theme } }));
    }

    // Update theme buttons
    function updateAllThemeButtons(theme) {
        const buttons = document.querySelectorAll('.theme-toggle');

        buttons.forEach(button => {
            const icon = button.querySelector('i');
            if (icon) {
                icon.style.opacity = '0';
                icon.style.transform = 'rotate(90deg)';

                setTimeout(() => {
                    if (theme === THEME_DARK) {
                        icon.className = 'fas fa-sun';
                    } else {
                        icon.className = 'fas fa-moon';
                    }

                    icon.style.opacity = '1';
                    icon.style.transform = 'rotate(0deg)';
                    icon.style.transition = 'opacity 0.25s ease, transform 0.25s ease';
                }, 125);
            }
        });
    }

    // Toggle theme globally
    function toggleTheme() {
        const current = document.documentElement.getAttribute('data-theme') || THEME_LIGHT;
        const newTheme = current === THEME_LIGHT ? THEME_DARK : THEME_LIGHT;
        applyTheme(newTheme);
    }

    // Handle storage changes from other tabs
    function onStorageChange(event) {
        if (event.key === STORAGE_KEY && event.newValue) {
            applyTheme(event.newValue);
        }
    }

    // Get current theme
    function getCurrentTheme() {
        return document.documentElement.getAttribute('data-theme') || THEME_LIGHT;
    }

    // Expose functions globally
    window.toggleTheme = toggleTheme;
    window.getCurrentTheme = getCurrentTheme;
    window.applyTheme = applyTheme;
    window.THEME_MANAGER = {
        toggle: toggleTheme,
        getCurrent: getCurrentTheme,
        set: applyTheme,
        LIGHT: THEME_LIGHT,
        DARK: THEME_DARK
    };

    // Initialize when DOM is ready
    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initTheme);
    } else {
        initTheme();
    }

})();

/* ==========================================
   NAVBAR TOGGLE - Push Content Down
   ========================================== */

(function () {
    'use strict';

    function initNavbarToggle() {
        const mobileMenuToggle = document.getElementById('mobileMenuToggle');
        const mobileMenu = document.getElementById('mobileMenu');
        const tabContents = document.querySelectorAll('.tab-content, [role="tabpanel"]');

        if (!mobileMenuToggle || !mobileMenu) return;

        function getMenuHeight() {
            return mobileMenu.scrollHeight || mobileMenu.offsetHeight || 0;
        }

        window.toggleMobileMenuWithPush = function () {
            const isExpanded = mobileMenu.classList.contains('show');

            if (isExpanded) {
                collapseMenu();
            } else {
                expandMenu();
            }
        };

        function expandMenu() {
            mobileMenu.classList.add('show');
            mobileMenuToggle.classList.add('active');

            const menuHeight = getMenuHeight();
            tabContents.forEach(content => {
                if (content) {
                    content.style.marginTop = (65 + menuHeight) + 'px';
                    content.style.transition = 'margin-top 0.3s ease';
                }
            });
        }

        function collapseMenu() {
            mobileMenu.classList.remove('show');
            mobileMenuToggle.classList.remove('active');

            tabContents.forEach(content => {
                if (content) {
                    content.style.marginTop = '65px';
                    content.style.transition = 'margin-top 0.3s ease';
                }
            });
        }

        window.closeMobileMenuWithPush = collapseMenu;

        document.addEventListener('click', function (event) {
            if (!mobileMenu.contains(event.target) &&
                !mobileMenuToggle.contains(event.target) &&
                mobileMenu.classList.contains('show')) {
                collapseMenu();
            }
        });
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initNavbarToggle);
    } else {
        initNavbarToggle();
    }

})();

/* ==========================================
   UTILITY FUNCTIONS
   ========================================== */

function showLoader() {
    const loader = document.getElementById('globalLoader');
    if (loader) {
        loader.style.display = 'flex';
    }
}

function hideLoader() {
    const loader = document.getElementById('globalLoader');
    if (loader) {
        setTimeout(() => {
            loader.style.display = 'none';
        }, 300);
    }
}

window.showLoader = showLoader;
window.hideLoader = hideLoader;

/* ==========================================
   AUTO-APPLY THEME ON PAGE LOAD
   ========================================== */

(function () {
    const theme = localStorage.getItem('appTheme') || 'light';
    document.documentElement.setAttribute('data-theme', theme);
})();

/* ==========================================
   SYNC THEME ACROSS BROWSER TABS
   ========================================== */

window.addEventListener('focus', function () {
    const savedTheme = localStorage.getItem('appTheme') || 'light';
    const currentTheme = document.documentElement.getAttribute('data-theme');

    if (savedTheme !== currentTheme) {
        document.documentElement.setAttribute('data-theme', savedTheme);
    }
});
