/* مدیر مالی شخصی — اسکریپتهای عمومی رابط کاربری */
(function () {
    'use strict';

    // ---------- Chart.js defaults ----------
    if (window.Chart) {
        Chart.defaults.font.family = "'Vazirmatn', 'Segoe UI', Tahoma, sans-serif";
        Chart.defaults.font.size = 12;
        Chart.defaults.color = '#6b7a90';
        Chart.defaults.plugins.legend.rtl = true;
        Chart.defaults.plugins.legend.labels.usePointStyle = true;
        Chart.defaults.plugins.tooltip.rtl = true;
        Chart.defaults.plugins.tooltip.textDirection = 'rtl';
    }

    // ---------- Shared helpers ----------
    window.FM = {
        palette: ['#2f6fed', '#0ca678', '#e8930c', '#7048e8', '#e03131', '#1098ad', '#f06595', '#82c91e', '#f59f00', '#495057'],

        formatMoney: function (minorUnits, decimals, currencyName) {
            var value = minorUnits / Math.pow(10, decimals);
            var text = new Intl.NumberFormat('fa-IR', {
                minimumFractionDigits: decimals,
                maximumFractionDigits: decimals
            }).format(value);
            return currencyName ? text + ' ' + currencyName : text;
        },

        formatNumber: function (value) {
            return new Intl.NumberFormat('fa-IR').format(value);
        }
    };

    document.addEventListener('DOMContentLoaded', function () {
        // ---------- Sidebar (mobile) ----------
        var sidebar = document.getElementById('appSidebar');
        var backdrop = document.getElementById('sidebarBackdrop');
        var toggle = document.getElementById('sidebarToggle');

        function closeSidebar() {
            if (sidebar) { sidebar.classList.remove('open'); }
            if (backdrop) { backdrop.classList.remove('show'); }
        }

        if (toggle) {
            toggle.addEventListener('click', function () {
                if (sidebar) { sidebar.classList.toggle('open'); }
                if (backdrop) { backdrop.classList.toggle('show'); }
            });
        }

        if (backdrop) {
            backdrop.addEventListener('click', closeSidebar);
        }

        // ---------- Period selector: toggle custom date fields ----------
        document.querySelectorAll('[data-period-select]').forEach(function (select) {
            var form = select.closest('form');
            if (!form) { return; }
            var fields = form.querySelectorAll('[data-custom-field]');

            function sync() {
                var isCustom = select.value === '4';
                fields.forEach(function (field) {
                    field.classList.toggle('d-none', !isCustom);
                    field.querySelectorAll('input, select').forEach(function (input) {
                        input.disabled = !isCustom;
                    });
                });
            }

            select.addEventListener('change', sync);
            sync();
        });

        // ---------- Transaction form: currency hint, card/category filtering ----------
        var accountSelect = document.querySelector('[data-accounts-select]');
        if (accountSelect) {
            var cardSelect = document.querySelector('[data-cards-select]');
            var currencyHint = document.querySelector('[data-currency-hint]');

            function syncAccount() {
                var option = accountSelect.selectedOptions[0];
                if (currencyHint) {
                    currencyHint.textContent = option && option.dataset.currencyName
                        ? 'واحد رقم ورودی: ' + option.dataset.currencyName
                        : '';
                }

                if (cardSelect) {
                    cardSelect.querySelectorAll('option[data-account-id]').forEach(function (o) {
                        var match = o.dataset.accountId === accountSelect.value;
                        o.hidden = !match;
                        o.disabled = !match;
                    });
                    var current = cardSelect.selectedOptions[0];
                    if (current && current.hidden) { cardSelect.value = ''; }
                }
            }

            accountSelect.addEventListener('change', syncAccount);
            syncAccount();
        }

        var typeSelect = document.querySelector('[data-type-select]');
        if (typeSelect) {
            var categorySelect = document.querySelector('[data-categories-select]');

            function syncType() {
                if (!categorySelect) { return; }
                var type = typeSelect.value; // ۱=واریز ، ۲=برداشت
                categorySelect.querySelectorAll('option[data-kind]').forEach(function (o) {
                    var kind = o.dataset.kind; // ۰=عمومی ، ۱=فقط برداشت ، ۲=فقط واریز
                    var ok = kind === '0'
                        || (type === '1' && kind === '2')
                        || (type === '2' && kind === '1');
                    o.hidden = !ok;
                    o.disabled = !ok;
                });
                var current = categorySelect.selectedOptions[0];
                if (current && current.hidden) { categorySelect.value = ''; }
            }

            typeSelect.addEventListener('change', syncType);
            syncType();
        }

        // ---------- Transfer form: destination filtered by source currency ----------
        var transferFrom = document.querySelector('[data-transfer-from]');
        var transferTo = document.querySelector('[data-transfer-to]');
        if (transferFrom && transferTo) {
            function syncTransfer() {
                var fromOption = transferFrom.selectedOptions[0];
                var currency = fromOption ? fromOption.dataset.currency : null;

                transferTo.querySelectorAll('option[data-currency]').forEach(function (o) {
                    var match = o.dataset.currency === currency && o.value !== transferFrom.value;
                    o.hidden = !match;
                    o.disabled = !match;
                });

                var current = transferTo.selectedOptions[0];
                if (current && current.hidden) { transferTo.value = ''; }
            }

            transferFrom.addEventListener('change', syncTransfer);
            syncTransfer();
        }
    });
})();
