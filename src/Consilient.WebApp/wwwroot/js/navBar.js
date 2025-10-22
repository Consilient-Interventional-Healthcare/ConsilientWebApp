(function() {
    const html = document.documentElement;

    // Mobile elements
    const openBtn   = document.getElementById('mobile-menu-open');
    const closeBtn  = document.getElementById('mobile-menu-close');
    const panel     = document.getElementById('mobile-menu-panel');
    const overlay   = document.getElementById('mobile-overlay');

    // Desktop admin dropdown
    const adminBtn      = document.getElementById('admin-desktop-btn');
    const adminMenu     = document.getElementById('admin-desktop-menu');
    const adminWrapper  = document.getElementById('admin-desktop-wrapper');

    // Mobile admin accordion
    const adminMobileBtn  = document.getElementById('admin-mobile-btn');
    const adminMobileMenu = document.getElementById('admin-mobile-menu');

    // Helpers
    const openMobile = () => {
        panel.classList.remove('translate-x-full');
        overlay.classList.remove('hidden');
        html.style.overflow = 'hidden'; // scroll lock
        openBtn?.setAttribute('aria-expanded', 'true');
    };
    const closeMobile = () => {
        panel.classList.add('translate-x-full');
        overlay.classList.add('hidden');
        html.style.overflow = ''; // restore
        openBtn?.setAttribute('aria-expanded', 'false');
    };

    // Open/close mobile panel
    openBtn?.addEventListener('click', openMobile);
    closeBtn?.addEventListener('click', closeMobile);
    overlay?.addEventListener('click', closeMobile);

    // Close after any link click inside mobile
    panel?.querySelectorAll('[data-close-mobile]').forEach(a => {
        a.addEventListener('click', () => setTimeout(closeMobile, 0));
    });

    // Escape closes mobile and desktop dropdown
    document.addEventListener('keydown', (e) => {
        if (e.key === 'Escape')
        {
            closeMobile();
            if (adminMenu && !adminMenu.classList.contains('hidden'))
            {
                adminMenu.classList.add('hidden');
                adminBtn?.setAttribute('aria-expanded', 'false');
            }
        }
    });

    // Desktop Admin dropdown toggle (click-to-open, click-away close)
    if (adminBtn && adminMenu && adminWrapper)
    {
        const toggleAdmin = () => {
            const isOpen = !adminMenu.classList.contains('hidden');
            adminMenu.classList.toggle('hidden', isOpen);
            adminBtn.setAttribute('aria-expanded', String(!isOpen));
        };
        adminBtn.addEventListener('click', (e) => {
            e.stopPropagation();
            toggleAdmin();
        });
        document.addEventListener('click', (e) => {
            if (!adminWrapper.contains(e.target))
            {
                adminMenu.classList.add('hidden');
                adminBtn.setAttribute('aria-expanded', 'false');
            }
        });
    }

    // Mobile Admin accordion
    if (adminMobileBtn && adminMobileMenu)
    {
        adminMobileBtn.addEventListener('click', () => {
            const isHidden = adminMobileMenu.classList.contains('hidden');
            adminMobileMenu.classList.toggle('hidden', !isHidden);
            adminMobileBtn.setAttribute('aria-expanded', String(isHidden));
        });
    }
})();
