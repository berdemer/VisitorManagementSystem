// Visitor Management System - Main Application
class VisitorApp {
    constructor() {
        this.apiBase = '/api';
        this.token = null;
        this.user = null;
        this.refreshInterval = null;
        this.init();
    }

    init() {
        this.checkAuth();
        this.bindEvents();
        this.loadActiveVisitors();
        this.startAutoRefresh();
        this.initResidentFeatures();
    }

    checkAuth() {
        this.token = localStorage.getItem('authToken');
        const userStr = localStorage.getItem('user');
        
        if (this.token && userStr) {
            try {
                this.user = JSON.parse(userStr);
                this.updateUIForLoggedInUser();
            } catch (error) {
                console.error('Auth check error:', error);
                this.clearAuth();
            }
        } else {
            this.updateUIForAnonymousUser();
        }
    }

    updateUIForLoggedInUser() {
        // Update navbar to show user info
        const navbar = document.querySelector('.navbar-nav.ms-auto');
        if (navbar && this.user) {
            // Add dropdown class to navbar
            navbar.className = 'navbar-nav ms-auto position-relative dropdown';
            
            // Create the new navbar content
            let navbarContent = `
                <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown" aria-expanded="false">
                    <i class="bi bi-person-circle"></i> ${this.escapeHtml(this.user.fullName || this.user.username)}
                </a>
                <ul class="dropdown-menu dropdown-menu-end">
                    <li>
                        <h6 class="dropdown-header">
                            <i class="bi bi-person-badge"></i> ${this.escapeHtml(this.user.fullName || this.user.username)}
                            <br><small class="text-muted">${this.getRoleDisplayName(this.user.role)}</small>
                        </h6>
                    </li>
                    <li><hr class="dropdown-divider"></li>
            `;

            // Add admin panel link for admin/manager users
            if (this.user.role === 'Admin' || this.user.role === 'Manager') {
                navbarContent += `
                        <li>
                            <a class="dropdown-item" href="admin.html">
                                <i class="bi bi-gear"></i> Yönetim Paneli
                            </a>
                        </li>
                        <li><hr class="dropdown-divider"></li>
                `;
            }

            // Add logout option
            navbarContent += `
                        <li>
                            <a class="dropdown-item text-danger" href="#" id="logoutLink">
                                <i class="bi bi-box-arrow-right"></i> Çıkış Yap
                            </a>
                        </li>
                    </ul>
            `;

            // Set the navbar content
            navbar.innerHTML = navbarContent;

            // Bind logout event
            const logoutLink = document.getElementById('logoutLink');
            if (logoutLink) {
                logoutLink.addEventListener('click', (e) => {
                    e.preventDefault();
                    this.logout();
                });
            }

            // Initialize Bootstrap dropdown or add manual toggle
            setTimeout(() => {
                const dropdownElement = navbar.querySelector('[data-bs-toggle="dropdown"]');
                const dropdownMenu = navbar.querySelector('.dropdown-menu');
                
                if (dropdownElement && dropdownMenu) {
                    if (window.bootstrap) {
                        // Use Bootstrap dropdown
                        new bootstrap.Dropdown(dropdownElement);
                    } else {
                        // Fallback manual toggle
                        dropdownElement.addEventListener('click', (e) => {
                            e.preventDefault();
                            dropdownMenu.classList.toggle('show');
                        });
                        
                        // Hide dropdown when clicking outside
                        document.addEventListener('click', (e) => {
                            if (!dropdownElement.contains(e.target) && !dropdownMenu.contains(e.target)) {
                                dropdownMenu.classList.remove('show');
                            }
                        });
                    }
                }
            }, 100);
        }

    }

    updateUIForAnonymousUser() {
        // Reset navbar to original state
        const navbar = document.querySelector('.navbar-nav.ms-auto, .navbar-nav.ms-auto.position-relative.dropdown');
        if (navbar) {
            navbar.className = 'navbar-nav ms-auto position-relative';
            navbar.innerHTML = `
                <a class="nav-link" href="login.html">
                    <i class="bi bi-person"></i> Giriş
                </a>
            `;
        }

    }

    bindEvents() {
        // Visitor form submission
        const visitorForm = document.getElementById('visitorForm');
        if (visitorForm) {
            visitorForm.addEventListener('submit', (e) => {
                e.preventDefault();
                this.createVisitor();
            });
        }

        // Photo capture events
        const captureBtn = document.getElementById('captureBtn');
        if (captureBtn) {
            captureBtn.addEventListener('click', () => {
                const photoFile = document.getElementById('photoFile');
                if (photoFile) {
                    photoFile.click();
                }
            });
        }

        const photoFile = document.getElementById('photoFile');
        if (photoFile) {
            photoFile.addEventListener('change', (e) => {
                this.previewPhoto(e.target.files[0]);
            });
        }

        // Refresh button
        const refreshBtn = document.getElementById('refreshBtn');
        if (refreshBtn) {
            refreshBtn.addEventListener('click', () => {
                this.loadActiveVisitors();
            });
        }

        // License plate uppercase transformation
        const licensePlate = document.getElementById('licensePlate');
        if (licensePlate) {
            licensePlate.addEventListener('input', (e) => {
                e.target.value = e.target.value.toUpperCase();
            });
        }

        // Phone number formatting with mask
        const phoneInputs = ['residentPhone', 'visitorPhone'];
        phoneInputs.forEach(inputId => {
            const phoneInput = document.getElementById(inputId);
            if (phoneInput) {
                phoneInput.addEventListener('input', (e) => {
                    this.formatPhoneNumber(e.target);
                });
            }
        });

        // Logout button (if exists)
        const logoutBtn = document.getElementById('logoutBtn');
        if (logoutBtn) {
            logoutBtn.addEventListener('click', (e) => {
                e.preventDefault();
                this.logout();
            });
        }

        // Call button
        const callBtn = document.getElementById('callBtn');
        if (callBtn) {
            callBtn.addEventListener('click', () => {
                this.makePhoneCall();
            });
        }

        // SMS button
        const sendSmsBtn = document.getElementById('sendSmsBtn');
        if (sendSmsBtn) {
            sendSmsBtn.addEventListener('click', () => {
                this.sendSmsVerification();
            });
        }

    }

    async createVisitor() {
        // Get form values
        const blockSelect = this.getInputValue('blockSelect');
        const subBlockSelect = this.getInputValue('subBlockSelect');
        const apartmentNumber = this.getInputValue('apartmentNumber');
        const residentName = this.getInputValue('residentName');
        const residentPhone = this.getInputValue('residentPhone');
        const fullName = this.getInputValue('fullName');
        const visitorPhone = this.getInputValue('visitorPhone');
        const licensePlate = this.getInputValue('licensePlate');
        const visitReason = this.getInputValue('visitReason');

        // Validate required fields
        if (!blockSelect || !subBlockSelect || !apartmentNumber) {
            await this.showAlert('Eksik Bilgi', 'Ana Blok, Alt Blok ve Daire No zorunludur', 'warning');
            return;
        }

        if (!residentName) {
            await this.showAlert('Eksik Bilgi', 'Daire sahibi adı zorunludur', 'warning');
            return;
        }

        if (!residentPhone) {
            await this.showAlert('Eksik Bilgi', 'Daire sahibi telefon numarası zorunludur', 'warning');
            return;
        }

        if (!fullName) {
            await this.showAlert('Eksik Bilgi', 'Ziyaretçi adı soyadı zorunludur', 'warning');
            return;
        }

        // Create full apartment number
        const fullApartmentNumber = `${blockSelect}${subBlockSelect}-${apartmentNumber}`;

        const visitorData = {
            fullName: fullName,
            apartmentNumber: fullApartmentNumber,
            residentName: residentName,
            residentPhone: residentPhone,
            visitorPhone: visitorPhone || null,
            licensePlate: licensePlate || null,
            visitReason: visitReason || 'Belirtilmemiş',
            notes: `Blok: ${blockSelect}${subBlockSelect}, Daire: ${apartmentNumber}, Daire Sahibi: ${residentName}`
        };

        try {
            const response = await this.apiCall('/visitor', 'POST', visitorData);

            if (response.ok) {
                const visitor = await response.json();
                
                // Upload photo if selected
                const photoFile = document.getElementById('photoFile');
                if (photoFile && photoFile.files[0]) {
                    await this.uploadPhoto(visitor.id, photoFile.files[0]);
                }

                this.resetForm();
                this.loadActiveVisitors();
                // Toast notification for success instead of modal
                this.showSuccess('Ziyaretçi kaydı başarıyla oluşturuldu');
            } else {
                const errorData = await response.json().catch(() => ({}));
                await this.showAlert('Hata', errorData.message || 'Ziyaretçi kaydı oluşturulamadı', 'error');
            }
        } catch (error) {
            console.error('Create visitor error:', error);
            await this.showAlert('Hata', 'Kayıt işlemi sırasında hata oluştu', 'error');
        }
    }

    async uploadPhoto(visitorId, photoFile) {
        const formData = new FormData();
        formData.append('photo', photoFile);

        try {
            const response = await fetch(`${this.apiBase}/visitor/upload-photo?visitorId=${visitorId}`, {
                method: 'POST',
                headers: this.token ? {
                    'Authorization': `Bearer ${this.token}`
                } : {},
                body: formData
            });

            if (!response.ok) {
                console.warn('Photo upload failed');
            }
        } catch (error) {
            console.error('Photo upload error:', error);
        }
    }

    async loadActiveVisitors() {
        try {
            const response = await this.apiCall('/visitor/active');

            if (response.ok) {
                const visitors = await response.json();
                this.displayActiveVisitors(visitors);
            } else {
                console.warn('Failed to load active visitors');
            }
        } catch (error) {
            console.error('Load active visitors error:', error);
        }
    }

    displayActiveVisitors(visitors) {
        const container = document.getElementById('activeVisitors');
        if (!container) return;
        
        if (visitors.length === 0) {
            container.innerHTML = `
                <div class="text-center text-muted">
                    <i class="bi bi-people fs-1"></i>
                    <p>Aktif ziyaretçi bulunmuyor</p>
                </div>
            `;
            return;
        }

        container.innerHTML = visitors.map(visitor => {
            // Extract visitor data fields
            const residentName = visitor.residentName;
            const residentPhone = visitor.residentPhone;
            const visitorPhone = visitor.visitorPhone;
            
            return `
            <div class="visitor-item mb-3 p-3 border rounded">
                <div class="d-flex justify-content-between align-items-start">
                    <div class="flex-grow-1">
                        <div class="fw-bold text-primary mb-2">${this.escapeHtml(visitor.fullName)}</div>
                        <div class="text-muted small">
                            <i class="bi bi-house"></i> ${this.escapeHtml(visitor.apartmentNumber)}
                            ${residentName ? `<br><i class="bi bi-person"></i> Daire Sahibi: ${this.escapeHtml(residentName)}` : ''}
                            ${residentPhone ? `<br><i class="bi bi-telephone"></i> Daire Tel: ${this.escapeHtml(residentPhone)}` : ''}
                            ${visitorPhone ? `<br><i class="bi bi-phone"></i> Ziyaretçi Tel: ${this.escapeHtml(visitorPhone)}` : ''}
                            ${visitor.licensePlate ? `<br><i class="bi bi-car-front"></i> Plaka: ${this.escapeHtml(visitor.licensePlate)}` : ''}
                            ${visitor.visitReason ? `<br><i class="bi bi-info-circle"></i> ${this.escapeHtml(visitor.visitReason)}` : ''}
                            ${visitor.photoPath ? `<br><a href="${this.escapeHtml(visitor.photoPath)}" target="_blank" class="photo-link"><i class="bi bi-image"></i> Fotoğraf görüntüle</a>` : ''}
                        </div>
                        <div class="text-muted small mt-2">
                            <i class="bi bi-clock"></i> Giriş: ${this.formatDateTime(visitor.checkInTime)}
                        </div>
                    </div>
                    <div class="d-flex flex-column gap-1">
                        <span class="badge bg-success">Aktif</span>
                        ${this.token ? `
                            <button class="btn btn-sm btn-warning" onclick="app.checkoutVisitor(${visitor.id})">
                                <i class="bi bi-box-arrow-right"></i> Çıkış
                            </button>
                        ` : ''}
                    </div>
                </div>
            </div>
            `;
        }).join('');
    }

    async checkoutVisitor(visitorId) {
        if (!this.token) {
            await this.showAlert('Yetki Hatası', 'Bu işlem için giriş yapmanız gerekir', 'error');
            return;
        }

        const confirmed = await this.showConfirm(
            'Ziyaretçi Çıkışı', 
            'Bu ziyaretçiyi çıkış yaptırmak istediğinizden emin misiniz?'
        );

        if (!confirmed) {
            return;
        }

        try {
            const response = await this.apiCall(`/visitor/${visitorId}/checkout`, 'POST');

            if (response.ok) {
                this.loadActiveVisitors();
                // Toast notification for success instead of modal
                this.showSuccess('Ziyaretçi çıkışı başarıyla kaydedildi');
            } else {
                await this.showAlert('Hata', 'Çıkış işlemi başarısız', 'error');
            }
        } catch (error) {
            console.error('Checkout error:', error);
            await this.showAlert('Hata', 'Çıkış işlemi sırasında hata oluştu', 'error');
        }
    }

    logout() {
        this.clearAuth();
        this.showSuccess('Çıkış yapıldı, login sayfasına yönlendiriliyor...');
        
        setTimeout(() => {
            window.location.href = 'login.html';
        }, 1500);
    }

    clearAuth() {
        this.token = null;
        this.user = null;
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
        this.updateUIForAnonymousUser();
    }

    previewPhoto(file) {
        if (!file) return;

        const reader = new FileReader();
        reader.onload = (e) => {
            const img = document.getElementById('previewImage');
            if (img) {
                img.src = e.target.result;
                img.classList.remove('d-none');
            }
        };
        reader.readAsDataURL(file);
    }

    resetForm() {
        const form = document.getElementById('visitorForm');
        if (form) {
            form.reset();
        }

        // Reset all select elements to default
        const selects = ['blockSelect', 'subBlockSelect', 'visitReason'];
        selects.forEach(selectId => {
            const element = document.getElementById(selectId);
            if (element) {
                element.value = '';
            }
        });

        const previewImage = document.getElementById('previewImage');
        if (previewImage) {
            previewImage.classList.add('d-none');
            previewImage.src = '';
        }
    }

    startAutoRefresh() {
        // Refresh active visitors every 30 seconds
        this.refreshInterval = setInterval(() => {
            this.loadActiveVisitors();
        }, 30000);
    }

    stopAutoRefresh() {
        if (this.refreshInterval) {
            clearInterval(this.refreshInterval);
            this.refreshInterval = null;
        }
    }

    // Utility methods
    async apiCall(endpoint, method = 'GET', body = null) {
        const options = {
            method,
            headers: {
                'Content-Type': 'application/json'
            }
        };

        // Add auth header if token exists
        if (this.token) {
            options.headers['Authorization'] = `Bearer ${this.token}`;
        }

        if (body) {
            options.body = JSON.stringify(body);
        }

        return fetch(`${this.apiBase}${endpoint}`, options);
    }

    getInputValue(id) {
        const element = document.getElementById(id);
        if (!element) return '';
        
        // For phone inputs, return clean value if available
        if ((id === 'residentPhone' || id === 'visitorPhone') && element.hasAttribute('data-clean-value')) {
            return element.getAttribute('data-clean-value');
        }
        
        return element.value.trim();
    }

    escapeHtml(text) {
        if (!text) return '';
        const map = {
            '&': '&amp;',
            '<': '&lt;',
            '>': '&gt;',
            '"': '&quot;',
            "'": '&#039;'
        };
        return text.replace(/[&<>"']/g, m => map[m]);
    }

    formatDateTime(dateString) {
        if (!dateString) return '';
        const date = new Date(dateString);
        return date.toLocaleString('tr-TR', {
            day: '2-digit',
            month: '2-digit',
            year: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
        });
    }

    showSuccess(message) {
        const toast = document.getElementById('successToast');
        const body = document.getElementById('successToastBody');
        
        if (toast && body) {
            body.textContent = message;
            const bsToast = new bootstrap.Toast(toast);
            bsToast.show();
        } else {
            // Fallback alert if toast elements don't exist
            alert(message);
        }
    }

    showError(message) {
        const toast = document.getElementById('errorToast');
        const body = document.getElementById('errorToastBody');
        
        if (toast && body) {
            body.textContent = message;
            const bsToast = new bootstrap.Toast(toast);
            bsToast.show();
        } else {
            // Fallback alert if toast elements don't exist
            alert(message);
        }
    }

    // Custom Modal Methods
    showModal(title, message, type = 'info', showCancel = false) {
        return new Promise((resolve) => {
            const modal = document.getElementById('customModal');
            const modalTitle = document.getElementById('customModalTitle');
            const modalMessage = document.getElementById('customModalMessage');
            const modalIcon = document.querySelector('#customModalLabel i');
            const confirmBtn = document.getElementById('customModalConfirmBtn');
            const cancelBtn = document.getElementById('customModalCancelBtn');
            
            if (!modal) {
                alert(message);
                resolve(false);
                return;
            }
            
            // Set content
            modalTitle.textContent = title;
            modalMessage.textContent = message;
            
            // Set icon and button styles based on type
            switch (type) {
                case 'error':
                    modalIcon.className = 'bi bi-exclamation-triangle text-danger me-2';
                    confirmBtn.className = 'btn btn-danger';
                    break;
                case 'warning':
                    modalIcon.className = 'bi bi-exclamation-triangle text-warning me-2';
                    confirmBtn.className = 'btn btn-warning';
                    break;
                case 'success':
                    modalIcon.className = 'bi bi-check-circle text-success me-2';
                    confirmBtn.className = 'btn btn-success';
                    break;
                default:
                    modalIcon.className = 'bi bi-info-circle text-primary me-2';
                    confirmBtn.className = 'btn btn-primary';
            }
            
            // Show/hide cancel button
            if (showCancel) {
                cancelBtn.style.display = 'block';
                confirmBtn.textContent = 'Tamam';
            } else {
                cancelBtn.style.display = 'none';
                confirmBtn.textContent = 'Tamam';
            }
            
            // Handle button clicks
            const handleConfirm = () => {
                resolve(true);
                bootstrap.Modal.getInstance(modal).hide();
                confirmBtn.removeEventListener('click', handleConfirm);
                cancelBtn.removeEventListener('click', handleCancel);
            };
            
            const handleCancel = () => {
                resolve(false);
                bootstrap.Modal.getInstance(modal).hide();
                confirmBtn.removeEventListener('click', handleConfirm);
                cancelBtn.removeEventListener('click', handleCancel);
            };
            
            confirmBtn.addEventListener('click', handleConfirm);
            cancelBtn.addEventListener('click', handleCancel);
            
            // Handle modal close
            modal.addEventListener('hidden.bs.modal', () => {
                resolve(false);
                confirmBtn.removeEventListener('click', handleConfirm);
                cancelBtn.removeEventListener('click', handleCancel);
            }, { once: true });
            
            // Show modal
            const bsModal = new bootstrap.Modal(modal);
            bsModal.show();
        });
    }

    // Convenience methods
    async showConfirm(title, message) {
        return await this.showModal(title, message, 'warning', true);
    }

    async showAlert(title, message, type = 'info') {
        return await this.showModal(title, message, type, false);
    }




    updateElement(id, value) {
        const element = document.getElementById(id);
        if (element) {
            element.textContent = value;
        }
    }

    getRoleDisplayName(role) {
        switch (role) {
            case 'Admin': return 'Sistem Yöneticisi';
            case 'Manager': return 'Yönetici';
            case 'Security': return 'Güvenlik';
            default: return role || 'Kullanıcı';
        }
    }

    initResidentFeatures() {
        // Apartment selection change handlers
        const blockSelect = document.getElementById('blockSelect');
        const subBlockSelect = document.getElementById('subBlockSelect');
        const apartmentNumber = document.getElementById('apartmentNumber');
        const residentName = document.getElementById('residentName');
        
        if (blockSelect) {
            blockSelect.addEventListener('change', () => this.onApartmentChange());
        }
        if (subBlockSelect) {
            subBlockSelect.addEventListener('change', () => this.onApartmentChange());
        }
        if (apartmentNumber) {
            apartmentNumber.addEventListener('input', () => this.onApartmentChange());
        }
        
        // Resident name autocomplete
        if (residentName) {
            residentName.addEventListener('input', (e) => this.onResidentNameInput(e));
            residentName.addEventListener('focus', () => this.showResidentSuggestions());
            residentName.addEventListener('blur', () => {
                // Hide suggestions after a short delay to allow clicking
                setTimeout(() => this.hideResidentSuggestions(), 200);
            });
        }

        // Visitor name autocomplete
        const visitorName = document.getElementById('fullName');
        if (visitorName) {
            visitorName.addEventListener('input', (e) => this.onVisitorNameInput(e));
            visitorName.addEventListener('focus', () => this.showVisitorSuggestions());
            visitorName.addEventListener('blur', () => {
                // Hide suggestions after a short delay to allow clicking
                setTimeout(() => this.hideVisitorSuggestions(), 200);
            });
        }
        
        // Hide suggestions when clicking outside
        document.addEventListener('click', (e) => {
            const residentSuggestions = document.getElementById('residentSuggestions');
            const residentNameInput = document.getElementById('residentName');
            const visitorSuggestions = document.getElementById('visitorSuggestions');
            const visitorNameInput = document.getElementById('fullName');
            
            if (residentSuggestions && residentNameInput && 
                !residentSuggestions.contains(e.target) && 
                !residentNameInput.contains(e.target)) {
                this.hideResidentSuggestions();
            }
            
            if (visitorSuggestions && visitorNameInput && 
                !visitorSuggestions.contains(e.target) && 
                !visitorNameInput.contains(e.target)) {
                this.hideVisitorSuggestions();
            }
        });
    }

    async onApartmentChange() {
        const blockSelect = this.getInputValue('blockSelect');
        const subBlockSelect = this.getInputValue('subBlockSelect');
        const apartmentNumber = this.getInputValue('apartmentNumber');
        
        if (blockSelect && subBlockSelect && apartmentNumber) {
            const fullApartmentNumber = `${blockSelect}${subBlockSelect}-${apartmentNumber}`;
            await this.loadResidentByApartment(fullApartmentNumber);
        } else {
            this.clearResidentInfo();
        }
    }

    async loadResidentByApartment(apartmentNumber) {
        try {
            const response = await this.apiCall(`/resident/apartment/${encodeURIComponent(apartmentNumber)}`);
            
            if (response.ok) {
                const resident = await response.json();
                this.populateResidentInfo(resident);
            } else {
                this.clearResidentInfo();
            }
        } catch (error) {
            console.error('Error loading resident:', error);
            this.clearResidentInfo();
        }
    }

    populateResidentInfo(resident) {
        const residentNameInput = document.getElementById('residentName');
        const residentPhoneInput = document.getElementById('residentPhone');
        const callBtn = document.getElementById('callBtn');
        
        if (residentNameInput) {
            residentNameInput.value = resident.fullName;
        }
        
        // Get the highest priority phone contact
        const phoneContact = resident.contacts && resident.contacts.length > 0 
            ? resident.contacts
                .filter(c => c.contactType === 'Phone' && c.isActive)
                .sort((a, b) => a.priority - b.priority)[0]
            : null;
        
        if (residentPhoneInput) {
            residentPhoneInput.value = phoneContact ? phoneContact.contactValue : '';
        }
        
        if (callBtn) {
            callBtn.disabled = !phoneContact;
        }
        
        this.hideResidentSuggestions();
    }

    clearResidentInfo() {
        const residentNameInput = document.getElementById('residentName');
        const residentPhoneInput = document.getElementById('residentPhone');
        const callBtn = document.getElementById('callBtn');
        
        if (residentNameInput && !residentNameInput.value.trim()) {
            residentNameInput.value = '';
        }
        
        if (residentPhoneInput) {
            residentPhoneInput.value = '';
        }
        
        if (callBtn) {
            callBtn.disabled = true;
        }
    }

    async onResidentNameInput(e) {
        const searchTerm = e.target.value.trim();
        
        if (searchTerm.length < 2) {
            this.hideResidentSuggestions();
            return;
        }
        
        try {
            const response = await this.apiCall('/resident/search', 'POST', {
                searchTerm: searchTerm,
                page: 1,
                pageSize: 10
            });
            
            if (response.ok) {
                const result = await response.json();
                this.showResidentSuggestions(result.residents || []);
            } else {
                this.hideResidentSuggestions();
            }
        } catch (error) {
            console.error('Error searching residents:', error);
            this.hideResidentSuggestions();
        }
    }

    showResidentSuggestions(residents = []) {
        const suggestionsContainer = document.getElementById('residentSuggestions');
        if (!suggestionsContainer) return;
        
        if (residents.length === 0) {
            this.hideResidentSuggestions();
            return;
        }
        
        const suggestionsHtml = residents.map(resident => {
            const phoneContact = resident.contacts && resident.contacts.length > 0 
                ? resident.contacts
                    .filter(c => c.contactType === 'Phone' && c.isActive)
                    .sort((a, b) => a.priority - b.priority)[0]
                : null;
            
            return `
                <div class="suggestion-item p-2 border-bottom cursor-pointer" 
                     onclick="app.selectResident(${resident.id}, '${this.escapeHtml(resident.fullName)}', '${resident.apartmentNumber}', '${phoneContact ? phoneContact.contactValue : ''}')">
                    <div class="fw-bold">${this.escapeHtml(resident.fullName)}</div>
                    <div class="text-muted small">
                        <i class="bi bi-building"></i> ${this.escapeHtml(resident.apartmentNumber)}
                        ${phoneContact ? `<i class="bi bi-telephone ms-2"></i> ${this.escapeHtml(phoneContact.contactValue)}` : ''}
                    </div>
                </div>
            `;
        }).join('');
        
        suggestionsContainer.innerHTML = suggestionsHtml;
        suggestionsContainer.classList.remove('d-none');
    }

    hideResidentSuggestions() {
        const suggestionsContainer = document.getElementById('residentSuggestions');
        if (suggestionsContainer) {
            suggestionsContainer.classList.add('d-none');
        }
    }

    selectResident(residentId, fullName, apartmentNumber, phoneNumber) {
        // Parse apartment number to fill form fields
        const apartmentMatch = apartmentNumber.match(/^([A-Z]+)(\d+)-(\d+)$/);
        if (apartmentMatch) {
            const [, block, subBlock, doorNumber] = apartmentMatch;
            
            const blockSelect = document.getElementById('blockSelect');
            const subBlockSelect = document.getElementById('subBlockSelect');
            const apartmentInput = document.getElementById('apartmentNumber');
            
            if (blockSelect) blockSelect.value = block;
            if (subBlockSelect) subBlockSelect.value = subBlock;
            if (apartmentInput) apartmentInput.value = doorNumber;
        }
        
        // Fill resident info
        const residentNameInput = document.getElementById('residentName');
        const residentPhoneInput = document.getElementById('residentPhone');
        const callBtn = document.getElementById('callBtn');
        
        if (residentNameInput) {
            residentNameInput.value = fullName;
        }
        
        if (residentPhoneInput && phoneNumber) {
            // Set the clean value first
            residentPhoneInput.value = phoneNumber;
            // Then format it
            this.formatPhoneNumber(residentPhoneInput);
        }
        
        if (callBtn) {
            callBtn.disabled = !phoneNumber;
        }
        
        this.hideResidentSuggestions();
    }

    async onVisitorNameInput(e) {
        const searchTerm = e.target.value.trim();
        
        if (searchTerm.length < 2) {
            this.hideVisitorSuggestions();
            return;
        }
        
        try {
            const response = await this.apiCall(`/visitor/search/${encodeURIComponent(searchTerm)}`);
            
            if (response.ok) {
                const visitors = await response.json();
                this.showVisitorSuggestions(visitors || []);
            } else {
                this.hideVisitorSuggestions();
            }
        } catch (error) {
            console.error('Error searching visitors:', error);
            this.hideVisitorSuggestions();
        }
    }

    showVisitorSuggestions(visitors = []) {
        const suggestionsContainer = document.getElementById('visitorSuggestions');
        if (!suggestionsContainer) return;
        
        if (visitors.length === 0) {
            this.hideVisitorSuggestions();
            return;
        }
        
        const suggestionsHtml = visitors.map(visitor => {
            return `
                <div class="suggestion-item p-2 border-bottom cursor-pointer" 
                     onclick="app.selectVisitor('${this.escapeHtml(visitor.fullName)}', '${visitor.visitorPhone || ''}', '${visitor.licensePlate || ''}')"
                     style="cursor: pointer;">
                    <div class="fw-bold">${this.escapeHtml(visitor.fullName)}</div>
                    <div class="text-muted small">
                        ${visitor.visitorPhone ? `<i class="bi bi-telephone"></i> ${this.escapeHtml(visitor.visitorPhone)}` : ''}
                        ${visitor.licensePlate ? `<i class="bi bi-car-front ms-2"></i> ${this.escapeHtml(visitor.licensePlate)}` : ''}
                        ${visitor.visitCount ? `<span class="badge bg-secondary ms-2">${visitor.visitCount} ziyaret</span>` : ''}
                    </div>
                </div>
            `;
        }).join('');
        
        suggestionsContainer.innerHTML = suggestionsHtml;
        suggestionsContainer.classList.remove('d-none');
    }

    hideVisitorSuggestions() {
        const suggestionsContainer = document.getElementById('visitorSuggestions');
        if (suggestionsContainer) {
            suggestionsContainer.classList.add('d-none');
        }
    }

    selectVisitor(fullName, visitorPhone, licensePlate) {
        // Fill visitor info
        const visitorNameInput = document.getElementById('fullName');
        const visitorPhoneInput = document.getElementById('visitorPhone');
        const licensePlateInput = document.getElementById('licensePlate');
        
        if (visitorNameInput) {
            visitorNameInput.value = fullName;
        }
        
        if (visitorPhoneInput && visitorPhone) {
            // Set the clean value first
            visitorPhoneInput.value = visitorPhone;
            // Then format it
            this.formatPhoneNumber(visitorPhoneInput);
        }
        
        if (licensePlateInput && licensePlate) {
            licensePlateInput.value = licensePlate;
        }
        
        this.hideVisitorSuggestions();
    }

    makePhoneCall() {
        const phoneNumber = this.getInputValue('residentPhone');
        if (phoneNumber) {
            // Format phone number for tel: protocol
            const cleanPhone = phoneNumber.replace(/\D/g, '');
            const formattedPhone = cleanPhone.startsWith('0') ? `+90${cleanPhone.substring(1)}` : `+90${cleanPhone}`;
            window.open(`tel:${formattedPhone}`);
        }
    }

    formatPhoneNumber(input) {
        // Get only digits
        let value = input.value.replace(/\D/g, '');
        
        // Limit to 11 digits for Turkish phone numbers
        if (value.length > 11) {
            value = value.substring(0, 11);
        }
        
        // Apply Turkish phone number formatting
        let formattedValue = '';
        
        if (value.length > 0) {
            // Start with 0
            if (value.length >= 1) {
                formattedValue = value.substring(0, 1);
            }
            
            // Add opening parenthesis and area code
            if (value.length >= 2) {
                formattedValue += ' (' + value.substring(1, 4);
            }
            
            // Add closing parenthesis after area code
            if (value.length >= 4) {
                formattedValue += ')';
            }
            
            // Add first part of number
            if (value.length >= 5) {
                formattedValue += ' ' + value.substring(4, 7);
            }
            
            // Add second part of number
            if (value.length >= 8) {
                formattedValue += ' ' + value.substring(7, 9);
            }
            
            // Add third part of number
            if (value.length >= 10) {
                formattedValue += ' ' + value.substring(9, 11);
            }
        }
        
        input.value = formattedValue;
        
        // Store the clean number for API calls
        input.setAttribute('data-clean-value', value);
    }

    async sendSmsVerification() {
        const phoneNumber = this.getInputValue('visitorPhone');
        
        if (!phoneNumber) {
            await this.showAlert('Eksik Bilgi', 'Ziyaretçi telefon numarası gereklidir', 'warning');
            return;
        }
        
        const sendSmsBtn = document.getElementById('sendSmsBtn');
        const smsCodeDisplay = document.getElementById('smsCodeDisplay');
        const smsCodeValue = document.getElementById('smsCodeValue');
        
        // Disable button during request
        if (sendSmsBtn) {
            sendSmsBtn.disabled = true;
            sendSmsBtn.innerHTML = '<i class="bi bi-hourglass-split"></i> Gönderiliyor...';
        }
        
        try {
            const response = await this.apiCall('/smsverification/send', 'POST', {
                phoneNumber: phoneNumber
            });
            
            if (response.ok) {
                const result = await response.json();
                
                if (result.success && result.code) {
                    // Show the SMS code
                    if (smsCodeValue) {
                        smsCodeValue.textContent = result.code;
                    }
                    
                    if (smsCodeDisplay) {
                        smsCodeDisplay.classList.remove('d-none');
                    }
                    
                    this.showToast('SMS gönderildi', 'success');
                } else {
                    await this.showAlert('Hata', result.message || 'SMS gönderilirken hata oluştu', 'error');
                }
            } else {
                const errorResult = await response.json();
                await this.showAlert('Hata', errorResult.message || 'SMS gönderilirken hata oluştu', 'error');
            }
        } catch (error) {
            console.error('SMS sending error:', error);
            await this.showAlert('Hata', 'SMS gönderilirken hata oluştu', 'error');
        } finally {
            // Re-enable button
            if (sendSmsBtn) {
                sendSmsBtn.disabled = false;
                sendSmsBtn.innerHTML = '<i class="bi bi-chat-dots"></i> SMS Gönder';
            }
        }
    }

    // Cleanup when page unloads
    destroy() {
        this.stopAutoRefresh();
    }
}

// Initialize app when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.app = new VisitorApp();
});

// Cleanup on page unload
window.addEventListener('beforeunload', () => {
    if (window.app) {
        window.app.destroy();
    }
});