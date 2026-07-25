// CodeSphere client-side helpers.
// Uses the Fetch API to talk to the CodeSphere.Api project without a full
// page reload — satisfies the "AJAX/Fetch API" bonus requirement for
// comments, reactions and bookmarks.

function csGetAntiForgeryToken() {
    const input = document.querySelector('input[name="__RequestVerificationToken"]');
    return input ? input.value : null;
}

async function csPostJson(url, body) {
    const token = csGetAntiForgeryToken();
    const headers = { 'Content-Type': 'application/json' };
    if (token) headers['RequestVerificationToken'] = token;

    const response = await fetch(url, {
        method: 'POST',
        headers,
        credentials: 'same-origin',
        body: JSON.stringify(body)
    });

    if (!response.ok) {
        const text = await response.text();
        throw new Error(text || `Request failed with status ${response.status}`);
    }
    const contentType = response.headers.get('content-type') || '';
    return contentType.includes('application/json') ? response.json() : null;
}

// ---------------------------------------------------------------------
// Toast notifications — a modern replacement for browser alert() popups.
// Renders a dismissible Bootstrap toast in the bottom-right corner instead
// of a blocking native alert box.
// ---------------------------------------------------------------------
function showToast(message, variant) {
    variant = variant || 'danger';
    let container = document.getElementById('csToastContainer');
    if (!container) {
        container = document.createElement('div');
        container.id = 'csToastContainer';
        container.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        container.style.zIndex = '1080';
        document.body.appendChild(container);
    }

    const toastEl = document.createElement('div');
    toastEl.className = `toast align-items-center text-bg-${variant} border-0`;
    toastEl.setAttribute('role', 'alert');
    toastEl.setAttribute('aria-live', 'assertive');
    toastEl.setAttribute('aria-atomic', 'true');
    toastEl.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">${message}</div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>`;
    container.appendChild(toastEl);

    if (window.bootstrap && window.bootstrap.Toast) {
        const toast = new bootstrap.Toast(toastEl, { delay: 5000 });
        toastEl.addEventListener('hidden.bs.toast', () => toastEl.remove());
        toast.show();
    } else {
        // Fallback if Bootstrap's JS hasn't loaded for some reason.
        toastEl.classList.add('show');
        setTimeout(() => toastEl.remove(), 5000);
    }
}

// ---------------------------------------------------------------------
// Tag picker — intellisense-style tag search used on the article
// create/edit pages instead of a giant checkbox list (there are 200+ tags).
// Renders selected tags as removable chips and keeps a hidden <input> per
// selected tag so the existing "Input.TagIds" model binding needs no
// server-side changes.
// ---------------------------------------------------------------------
function initTagPicker(options) {
    const input = document.getElementById(options.inputId);
    const chipsEl = document.getElementById(options.chipsId);
    const suggestionsEl = document.getElementById(options.suggestionsId);
    const hiddenContainer = document.getElementById(options.hiddenContainerId);
    if (!input || !chipsEl || !suggestionsEl || !hiddenContainer) return;

    const allTags = options.allTags || [];
    const selected = new Map();
    (options.initialSelected || []).forEach(t => selected.set(t.id, t.name));

    function renderChips() {
        chipsEl.innerHTML = '';
        hiddenContainer.innerHTML = '';
        selected.forEach((name, id) => {
            const chip = document.createElement('span');
            chip.className = 'tag-chip';
            chip.innerHTML = `#${name} <button type="button" class="tag-chip-remove" data-id="${id}" aria-label="Remove ${name}">&times;</button>`;
            chipsEl.appendChild(chip);

            const hidden = document.createElement('input');
            hidden.type = 'hidden';
            hidden.name = options.hiddenInputName;
            hidden.value = id;
            hiddenContainer.appendChild(hidden);
        });
        chipsEl.querySelectorAll('.tag-chip-remove').forEach(btn => {
            btn.addEventListener('click', () => {
                selected.delete(parseInt(btn.dataset.id, 10));
                renderChips();
            });
        });
    }

    function renderSuggestions(query) {
        suggestionsEl.innerHTML = '';
        if (!query) { suggestionsEl.classList.remove('show'); return; }
        const q = query.toLowerCase();
        const matches = allTags.filter(t => !selected.has(t.id) && t.name.toLowerCase().includes(q)).slice(0, 8);
        if (matches.length === 0) { suggestionsEl.classList.remove('show'); return; }
        matches.forEach(t => {
            const item = document.createElement('button');
            item.type = 'button';
            item.className = 'tag-suggestion-item';
            item.textContent = '#' + t.name;
            item.addEventListener('click', () => {
                selected.set(t.id, t.name);
                input.value = '';
                suggestionsEl.innerHTML = '';
                suggestionsEl.classList.remove('show');
                renderChips();
                input.focus();
            });
            suggestionsEl.appendChild(item);
        });
        suggestionsEl.classList.add('show');
    }

    input.addEventListener('input', () => renderSuggestions(input.value.trim()));
    input.addEventListener('focus', () => renderSuggestions(input.value.trim()));
    document.addEventListener('click', (e) => {
        if (!e.target.closest('[data-tag-picker]')) suggestionsEl.classList.remove('show');
    });
    input.addEventListener('keydown', (e) => {
        if (e.key === 'Enter') {
            e.preventDefault();
            const first = suggestionsEl.querySelector('.tag-suggestion-item');
            if (first) first.click();
        }
    });

    renderChips();
}

// ---------------------------------------------------------------------
// Dark / light theme toggle (Bootstrap 5.3 color modes), persisted in
// localStorage. Applied as early as possible (see the inline <head>
// script in _Layout.cshtml) to avoid a flash of the wrong theme; this
// function only wires up the toggle button itself.
// ---------------------------------------------------------------------
function csApplyTheme(theme) {
    document.documentElement.setAttribute('data-bs-theme', theme);
    try { localStorage.setItem('cs-theme', theme); } catch (e) { /* ignore (private browsing, etc.) */ }
    document.querySelectorAll('[data-theme-toggle] i').forEach(icon => {
        icon.className = theme === 'dark' ? 'bi bi-sun' : 'bi bi-moon-stars';
    });
}

function initThemeToggle() {
    const toggleBtn = document.querySelector('[data-theme-toggle]');
    if (!toggleBtn) return;
    const current = document.documentElement.getAttribute('data-bs-theme') || 'light';
    csApplyTheme(current);
    toggleBtn.addEventListener('click', () => {
        const next = document.documentElement.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
        csApplyTheme(next);
    });
}

// ---------------------------------------------------------------------
// Keep a CSS variable in sync with the actual (sticky) navbar height, so
// sticky table headers on the Reports page can dock right below it
// instead of overlapping or leaving a gap.
// ---------------------------------------------------------------------
function updateNavbarHeightVar() {
    const nav = document.querySelector('.cs-navbar');
    if (nav) {
        document.documentElement.style.setProperty('--navbar-height', nav.offsetHeight + 'px');
    }
}

document.addEventListener('DOMContentLoaded', () => {
    initThemeToggle();
    updateNavbarHeightVar();
    window.addEventListener('resize', updateNavbarHeightVar);

    // ---------------- Reactions (Like / Love / Idea) ----------------
    document.querySelectorAll('[data-reaction-btn]').forEach(btn => {
        btn.addEventListener('click', async () => {
            const articleId = btn.dataset.articleId;
            const type = btn.dataset.reactionBtn;
            try {
                const result = await csPostJson(`?handler=Reaction&articleId=${articleId}&type=${type}`, {});
                if (result) {
                    document.querySelectorAll('[data-reaction-count]').forEach(el => {
                        const rt = el.dataset.reactionCount;
                        el.textContent = result[rt] ?? 0;
                    });
                }
                btn.classList.toggle('active');
            } catch (e) {
                console.error(e);
                showToast('Could not register your reaction. Please try again.');
            }
        });
    });

    // ---------------- Bookmark toggle ----------------
    const bookmarkBtn = document.querySelector('[data-bookmark-btn]');
    if (bookmarkBtn) {
        bookmarkBtn.addEventListener('click', async () => {
            const articleId = bookmarkBtn.dataset.articleId;
            try {
                const bookmarked = await csPostJson(`?handler=Bookmark&articleId=${articleId}`, {});
                bookmarkBtn.classList.toggle('active', bookmarked === true);
                bookmarkBtn.innerHTML = bookmarked
                    ? '<i class="bi bi-bookmark-fill"></i> Saved'
                    : '<i class="bi bi-bookmark"></i> Save';
            } catch (e) {
                console.error(e);
                showToast('Could not update your bookmark. Please try again.');
            }
        });
    }

    // ---------------- Inline comment submit (no full page reload) ----------------
    const commentForm = document.querySelector('#inlineCommentForm');
    if (commentForm) {
        commentForm.addEventListener('submit', async (e) => {
            e.preventDefault();
            const articleId = commentForm.dataset.articleId;
            const textArea = commentForm.querySelector('textarea[name="commentText"]');
            const text = textArea.value.trim();
            if (!text) return;

            try {
                const comment = await csPostJson(`?handler=Comment&articleId=${articleId}`, { commentText: text });
                if (comment) {
                    const list = document.querySelector('#commentsList');
                    const item = document.createElement('div');
                    item.className = 'border-bottom py-2';
                    item.innerHTML = `<strong><a href="/Users/Details/${comment.userID}" class="text-decoration-none">${comment.author}</a></strong>
                        <span class="text-muted small">just now</span>
                        <p class="mb-0">${comment.commentText}</p>`;
                    list.prepend(item);
                    textArea.value = '';
                    const countEl = document.querySelector('#commentCount');
                    if (countEl) countEl.textContent = (parseInt(countEl.textContent || '0', 10) + 1).toString();
                }
            } catch (err) {
                console.error(err);
                showToast('Could not post your comment. Please try again.');
            }
        });
    }
});
