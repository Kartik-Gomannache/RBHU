/* site.js — vanilla JS with Global Loader
   - AOS init (if present)
   - Navbar scroll glass effect
   - Smooth anchors
   - Form enhancements (floating labels, email/phone helpers)
   - Stats counter, lazy loading
   - Product card hover
   - Scroll-to-top button, parallax, debounce utilities
   - GLOBAL LOADER functionality
*/

(function () {
    "use strict";

    // ===== Helpers =====
    const $ = (sel, root) => (root || document).querySelector(sel);
    const $$ = (sel, root) => Array.prototype.slice.call((root || document).querySelectorAll(sel));

    function debounce(fn, wait) {
        let t;
        return function () {
            clearTimeout(t);
            t = setTimeout(() => fn.apply(this, arguments), wait);
        };
    }

    // ===== GLOBAL LOADER FUNCTIONS =====
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

    // ===== Auto Page Load/Unload Loader =====
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

    // ===== AJAX Helper with Loader =====
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

    // ===== Form Submit with Loader =====
    function initFormLoader() {
        $$('form').forEach(form => {
            // Skip forms with data-no-loader attribute
            if (form.hasAttribute('data-no-loader')) return;

            form.addEventListener('submit', function (e) {
                const submitButton = form.querySelector('button[type="submit"], input[type="submit"]');
                const loadingMessage = form.getAttribute('data-loader-message') || 'Submitting...';

                // Only show loader if form doesn't have its own AJAX handling
                if (!form.hasAttribute('data-ajax')) {
                    showLoader(loadingMessage);
                }
            });
        });
    }

    // ===== AOS Init =====
    function initAOS() {
        if (typeof AOS !== "undefined") {
            AOS.init({ duration: 1000, easing: "ease-in-out", once: true, mirror: false });
        }
    }

    // ===== Navbar scroll effect =====
    function handleNavbarScroll() {
        const navbar = $(".navbar");
        if (!navbar) return;
        if (window.scrollY > 50) {
            navbar.style.background = "rgba(26, 54, 93, 0.98)";
            navbar.style.backdropFilter = "blur(15px)";
            navbar.style.boxShadow = "0 4px 30px rgba(0,0,0,0.2)";
        } else {
            navbar.style.background = "rgba(26, 54, 93, 0.95)";
            navbar.style.backdropFilter = "blur(10px)";
            navbar.style.boxShadow = "0 2px 20px rgba(0,0,0,0.1)";
        }
    }

    // ===== Smooth anchors =====
    function initSmoothScrolling() {
        $$('a[href^="#"]').forEach(anchor => {
            anchor.addEventListener("click", function (e) {
                const href = this.getAttribute("href");
                if (!href || href === "#") return;
                const target = document.querySelector(href);
                if (!target) return;
                e.preventDefault();
                target.scrollIntoView({ behavior: "smooth", block: "start" });
            });
        });
    }

    // ===== Form UX =====
    function initFormEnhancements() {
        $$(".form-control").forEach(input => {
            const parent = input.parentElement;
            if (input.value) parent?.classList.add("focused");
            input.addEventListener("focus", () => parent?.classList.add("focused"));
            input.addEventListener("blur", () => { if (!input.value) parent?.classList.remove("focused"); });
        });

        // Basic phone formatting (truncate to 10 digits, format 3-3-4)
        $$('input[type="tel"]').forEach(input => {
            input.addEventListener("input", e => {
                let v = e.target.value.replace(/\D/g, "");
                if (v.length > 10) v = v.substring(0, 10);
                if (v.length > 6) v = v.replace(/(\d{3})(\d{3})(\d{1,4})/, "$1-$2-$3");
                else if (v.length > 3) v = v.replace(/(\d{3})(\d{1,3})/, "$1-$2");
                e.target.value = v;
            });
        });

        // Email quick check (server still validates)
        $$('input[type="email"]').forEach(input => {
            input.addEventListener("blur", function () {
                const email = (this.value || "").trim();
                const ok = /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
                this.classList.toggle("is-invalid", !!email && !ok);
                const feedback = this.parentNode.querySelector(".invalid-feedback");
                if (feedback) {
                    feedback.textContent = (!!email && !ok) ? "Please enter a valid email address" : "";
                }
            });
        });
    }

    // ===== Stats counter =====
    function initStatsCounter() {
        const stats = $$(".stat-number");
        if (!("IntersectionObserver" in window) || !stats.length) return;

        const observer = new IntersectionObserver(entries => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    animateNumber(entry.target);
                    observer.unobserve(entry.target);
                }
            });
        });
        stats.forEach(s => observer.observe(s));
    }

    function animateNumber(el) {
        const finalNumber = parseInt(el.textContent.replace(/\D/g, ""), 10) || 0;
        let current = 0;
        const step = Math.max(1, Math.floor(finalNumber / 100));
        const suffix = el.textContent.replace(/[\d,]/g, "");
        const timer = setInterval(() => {
            current += step;
            if (current >= finalNumber) { current = finalNumber; clearInterval(timer); }
            el.textContent = current.toLocaleString() + suffix;
        }, 20);
    }

    // ===== Lazy images =====
    function initLazyLoading() {
        const imgs = $$("img[data-src]");
        if (!("IntersectionObserver" in window) || !imgs.length) return;

        const io = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    const img = entry.target;
                    img.src = img.dataset.src;
                    img.classList.remove("lazy");
                    io.unobserve(img);
                }
            });
        });
        imgs.forEach(img => io.observe(img));
    }

    // ===== Product hover =====
    function initProductHover() {
        $$(".product-card").forEach(card => {
            card.addEventListener("mouseenter", function () {
                this.style.transform = "translateY(-10px) scale(1.02)";
                this.style.boxShadow = "0 20px 50px rgba(0,0,0,0.2)";
            });
            card.addEventListener("mouseleave", function () {
                this.style.transform = "translateY(0) scale(1)";
                this.style.boxShadow = "0 5px 20px rgba(0,0,0,0.1)";
            });
        });
    }

    // ===== Parallax (hero background) =====
    function initParallax() {
        const hero = $(".hero-section");
        if (!hero) return;
        window.addEventListener("scroll", () => {
            const rate = window.pageYOffset * -0.5;
            hero.style.backgroundPosition = `center ${rate}px`;
        });
    }

    // ===== Boot =====
    document.addEventListener("DOMContentLoaded", function () {
        // Initialize page loader first
        initPageLoader();

        // Initialize other components
        initAOS();
        handleNavbarScroll();
        initSmoothScrolling();
        initFormEnhancements();
        initFormLoader();
        initStatsCounter();
        initLazyLoading();
        initProductHover();
        initParallax();

        window.addEventListener("scroll", debounce(handleNavbarScroll, 10));
    });
})();
