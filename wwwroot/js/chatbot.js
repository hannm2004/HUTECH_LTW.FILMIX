/* ══════════════════════════════════════════════════════════
   FILMIX CHATBOT  –  chatbot.js
   Client-side logic for the floating AI chat widget
══════════════════════════════════════════════════════════ */
(function () {
    'use strict';

    let isOpen    = false;
    let isBusy    = false;
    let msgCount  = 0;

    /* ── Toggle panel open/close ──────────────────────────── */
    window.chatbotToggle = function () {
        isOpen = !isOpen;
        const panel     = document.getElementById('chatbotPanel');
        const iconOpen  = document.getElementById('chatbotToggleIcon');
        const iconClose = document.getElementById('chatbotToggleClose');

        panel.classList.toggle('open', isOpen);
        iconOpen.style.display  = isOpen ? 'none'  : 'inline-flex';
        iconClose.style.display = isOpen ? 'inline-flex' : 'none';

        if (isOpen) {
            setTimeout(() => document.getElementById('chatbotInput')?.focus(), 200);
        }
    };

    /* ── Send a message ───────────────────────────────────── */
    window.chatbotSend = function (preset) {
        if (isBusy) return;

        const input = document.getElementById('chatbotInput');
        const text  = (preset || input.value || '').trim();
        if (!text) return;

        // Clear input
        if (!preset) input.value = '';

        // Append user bubble
        appendMsg('user', escapeHtml(text));
        hideSuggestions();

        // Show typing indicator
        const typingId = appendTyping();
        isBusy = true;
        toggleSendBtn(false);

        // Call API
        fetch('/api/chatbot/message', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ message: text })
        })
        .then(r => r.json())
        .then(data => {
            removeTyping(typingId);
            appendMsg('bot', formatReply(data.reply || 'Có lỗi xảy ra. Vui lòng thử lại.'));
        })
        .catch(() => {
            removeTyping(typingId);
            appendMsg('bot', '⚠️ Không thể kết nối. Vui lòng kiểm tra mạng và thử lại.');
        })
        .finally(() => {
            isBusy = false;
            toggleSendBtn(true);
        });
    };

    /* ── Append a message bubble ──────────────────────────── */
    function appendMsg(role, htmlContent) {
        const container = document.getElementById('chatbotMessages');
        if (!container) return;

        const row    = document.createElement('div');
        const bubble = document.createElement('div');
        row.className    = 'chatbot-msg ' + role;
        bubble.className = 'chatbot-bubble';
        bubble.innerHTML = htmlContent;

        row.appendChild(bubble);
        container.appendChild(row);
        scrollToBottom();
        msgCount++;
    }

    /* ── Typing indicator ─────────────────────────────────── */
    function appendTyping() {
        const container = document.getElementById('chatbotMessages');
        const id = 'typing_' + Date.now();
        const row = document.createElement('div');
        row.className = 'chatbot-msg bot chatbot-typing';
        row.id = id;
        row.innerHTML = `<div class="chatbot-bubble">
            <span class="typing-dot"></span>
            <span class="typing-dot"></span>
            <span class="typing-dot"></span>
        </div>`;
        container.appendChild(row);
        scrollToBottom();
        return id;
    }

    function removeTyping(id) {
        const el = document.getElementById(id);
        if (el) el.remove();
    }

    /* ── Format bot reply (mini-markdown) ────────────────── */
    function formatReply(text) {
        return text
            // Bold: **text** → <strong>text</strong>
            .replace(/\*\*(.+?)\*\*/g, '<strong>$1</strong>')
            // Markdown links: [text](url)
            .replace(/\[([^\]]+)\]\(([^)]+)\)/g, '<a href="$2" target="_self">$1</a>')
            // Newlines → <br/>
            .replace(/\n/g, '<br/>');
    }

    /* ── Helper: escape HTML for user input ──────────────── */
    function escapeHtml(str) {
        return str.replace(/[&<>"']/g, c => ({
            '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;'
        }[c]));
    }

    function scrollToBottom() {
        const c = document.getElementById('chatbotMessages');
        if (c) { setTimeout(() => { c.scrollTop = c.scrollHeight; }, 50); }
    }

    function hideSuggestions() {
        const s = document.getElementById('chatbotSuggestions');
        if (s && msgCount === 0) { /* keep on first msg */ }
        else if (s) s.style.display = 'none';
    }

    function toggleSendBtn(enabled) {
        const btn = document.getElementById('chatbotSendBtn');
        if (btn) btn.disabled = !enabled;
    }
})();
