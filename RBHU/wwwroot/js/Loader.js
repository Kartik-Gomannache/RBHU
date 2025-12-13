/* loader.js — Global Loader Functionality
   - Show/hide loader with custom messages
   - Auto page load/unload loader
   - AJAX helper with loader integration
   - Form submit with loader
*/

(function () {
    "use strict";

    // ===== Helper Functions =====
    const $ = (sel) => document.querySelector(sel);
    const $$ = (sel) => Array.from(document.querySelectorAll(sel));

    // ===== GLOBAL LOADER FUNCTIONS =====

    /**
     * Show the global loader with an optional message
     * @param {string} message - Loading message to display (default: "Loading...")
     */
    window.showLoader = function (message = "Loading...") {
        const loader = $("#globalLoader");
        const wrapper = $(".wrapper");
        const loaderText = $(".loader-text");

        if (loader) {
            if (loaderText && message) {
                loaderText.textContent = message;
            }
            loader.classList.add("active");
        }
        if (wrapper) {
            wrapper.classList.add("loading");
        }
    };

    /**
     * Hide the global loader
     */
    window.hideLoader = function () {
        const loader = $("#globalLoader");
        const wrapper = $(".wrapper");

        if (loader) {
            loader.classList.remove("active");
        }
        if (wrapper) {
            wrapper.classList.remove("loading");
        }
    };

    // ===== AUTO PAGE LOAD/UNLOAD LOADER =====
    function initPageLoader() {
        // Show loader on page navigation
        window.addEventListener("beforeunload", function () {
            showLoader("Loading page...");
        });

        // Hide loader when page is fully loaded
        window.addEventListener("load", function () {
            // Small delay to ensure smooth transition
            setTimeout(hideLoader, 300);
        });
    }

    // ===== AJAX HELPER WITH LOADER =====

    /**
     * Fetch data with automatic loader handling
     * @param {Object} options - Configuration options
     * @param {string} options.url - URL to fetch
     * @param {string} options.method - HTTP method (default: 'GET')
     * @param {Object} options.data - Data to send (will be JSON stringified)
     * @param {Object} options.headers - Additional headers
     * @param {string} options.loadingMessage - Message to show while loading
     * @param {Function} options.onSuccess - Success callback
     * @param {Function} options.onError - Error callback
     * @param {Function} options.onComplete - Complete callback (always runs)
     */
    window.ajaxWithLoader = function (options) {
        const defaultOptions = {
            url: '',
            method: 'GET',
            data: null,
            headers: {},
            loadingMessage: 'Processing...',
            onSuccess: null,
            onError: null,
            onComplete: null
        };

        const config = { ...defaultOptions, ...options };

        showLoader(config.loadingMessage);

        fetch(config.url, {
            method: config.method,
            headers: {
                'Content-Type': 'application/json',
                ...config.headers
            },
            body: config.data ? JSON.stringify(config.data) : null
        })
            .then(response => {
                if (!response.ok) {
                    throw new Error(`HTTP error! status: ${response.status}`);
                }
                return response.json();
            })
            .then(data => {
                if (config.onSuccess) {
                    config.onSuccess(data);
                }
            })
            .catch(error => {
                console.error('AJAX Error:', error);
                if (config.onError) {
                    config.onError(error);
                }
            })
            .finally(() => {
                hideLoader();
                if (config.onComplete) {
                    config.onComplete();
                }
            });
    };

    // ===== FORM SUBMIT WITH LOADER =====
    function initFormLoader() {
        $$('form').forEach(form => {
            // Skip forms with data-no-loader attribute
            if (form.hasAttribute('data-no-loader')) return;

            form.addEventListener('submit', function (e) {
                const loadingMessage = form.getAttribute('data-loader-message') || 'Submitting...';

                // Only show loader if form doesn't have its own AJAX handling
                if (!form.hasAttribute('data-ajax')) {
                    showLoader(loadingMessage);
                }
            });
        });
    }

    // ===== INITIALIZATION =====
    document.addEventListener("DOMContentLoaded", function () {
        initPageLoader();
        initFormLoader();
    });

})();

/* 
USAGE EXAMPLES:

1. Manual loader control:
   showLoader("Please wait...");
   hideLoader();

2. AJAX with loader:
   ajaxWithLoader({
       url: '/api/data',
       method: 'POST',
       data: { name: 'John' },
       loadingMessage: 'Saving data...',
       onSuccess: function(data) {
           console.log('Success:', data);
       },
       onError: function(error) {
           console.error('Error:', error);
       }
   });

3. Form with custom loader message:
   <form data-loader-message="Processing your request...">
       <!-- form fields -->
   </form>

4. Form without loader:
   <form data-no-loader>
       <!-- form fields -->
   </form>

5. Form with AJAX (prevents auto-loader):
   <form data-ajax>
       <!-- handle loader manually in your AJAX code -->
   </form>
*/