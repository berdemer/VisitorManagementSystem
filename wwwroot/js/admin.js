// Admin Panel for Visitor Management System
class AdminApp {
    constructor() {
        this.apiBase = '/api';
        this.token = null;
        this.user = null;
        this.currentPage = 1;
        this.pageSize = 10;
        this.totalRecords = 0;
        this.currentSearchMode = 'all'; // 'all', 'search', 'licensePlate', 'general', 'apartment', 'block'
        this.lastSearchData = null;
        
        // Visitor pagination
        this.visitorCurrentPage = 1;
        this.visitorPageSize = 10;
        this.visitorTotalRecords = 0;
        this.visitorCurrentSearchMode = 'all';
        this.visitorLastSearchData = null;
        this.init();
    }

    init() {
        // Check authentication first
        if (!this.checkAuth()) {
            this.redirectToLogin();
            return;
        }

        this.bindEvents();
        this.loadUserInfo();
        this.loadInitialData();
    }

    checkAuth() {
        this.token = localStorage.getItem('authToken');
        const userStr = localStorage.getItem('user');
        
        if (!this.token || !userStr) {
            return false;
        }

        try {
            this.user = JSON.parse(userStr);
            
            // Check if user has admin/manager role
            if (this.user.role !== 'Admin' && this.user.role !== 'Manager') {
                this.redirectToLogin();
                return false;
            }
            
            return true;
        } catch (error) {
            console.error('Auth check error:', error);
            return false;
        }
    }

    redirectToLogin() {
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
        window.location.href = 'login.html';
    }

    bindEvents() {
        // Logout button
        const logoutBtn = document.getElementById('logoutBtn');
        if (logoutBtn) {
            logoutBtn.addEventListener('click', () => this.logout());
        }

        // Visitors tab events
        const filterDate = document.getElementById('filterDate');
        if (filterDate) {
            filterDate.addEventListener('change', () => {
                this.visitorCurrentPage = 1;
                this.searchVisitors();
            });
        }

        const filterApartment = document.getElementById('filterApartment');
        if (filterApartment) {
            filterApartment.addEventListener('input', () => {
                this.visitorCurrentPage = 1;
                this.searchVisitors();
            });
            
            filterApartment.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    this.visitorCurrentPage = 1;
                    this.searchVisitors();
                }
            });
        }

        const filterPlate = document.getElementById('filterPlate');
        const plateSearchBtn = document.getElementById('plateSearchBtn');
        
        if (filterPlate) {
            filterPlate.addEventListener('input', (e) => {
                e.target.value = e.target.value.toUpperCase();
                this.visitorCurrentPage = 1;
                this.searchVisitors();
            });
            
            filterPlate.addEventListener('keypress', (e) => {
                if (e.key === 'Enter') {
                    e.preventDefault();
                    this.visitorCurrentPage = 1;
                    this.searchVisitors();
                }
            });
        }
        
        if (plateSearchBtn) {
            plateSearchBtn.addEventListener('click', () => {
                this.visitorCurrentPage = 1;
                this.searchVisitors();
            });
        }

        const filterStatus = document.getElementById('filterStatus');
        if (filterStatus) {
            filterStatus.addEventListener('change', () => {
                this.visitorCurrentPage = 1;
                this.searchVisitors();
            });
        }

        // Visitor page size
        const visitorsPageSize = document.getElementById('visitorsPageSize');
        if (visitorsPageSize) {
            visitorsPageSize.addEventListener('change', (e) => {
                this.visitorPageSize = parseInt(e.target.value);
                this.visitorCurrentPage = 1;
                this.refreshVisitorCurrentView();
            });
        }

        // Reports tab events
        const generateReport = document.getElementById('generateReport');
        if (generateReport) {
            generateReport.addEventListener('click', () => this.generateReport());
        }

        const refreshApartmentStats = document.getElementById('refreshApartmentStats');
        if (refreshApartmentStats) {
            refreshApartmentStats.addEventListener('click', () => this.loadApartmentStats());
        }

        const updateApartmentStats = document.getElementById('updateApartmentStats');
        if (updateApartmentStats) {
            updateApartmentStats.addEventListener('click', () => this.loadApartmentStats());
        }

        // Residents tab events (Admin only)
        if (this.user.role === 'Admin') {
            const addResidentBtn = document.getElementById('addResidentBtn');
            if (addResidentBtn) {
                addResidentBtn.addEventListener('click', () => this.showResidentModal());
            }

            const searchResidents = document.getElementById('searchResidents');
            if (searchResidents) {
                searchResidents.addEventListener('click', () => this.searchResidents());
            }

            // Arama field'larını al
            const licensePlateSearch = document.getElementById('licensePlateSearch');
            const searchByPlateBtn = document.getElementById('searchByPlateBtn');
            const residentSearch = document.getElementById('residentSearch');
            const generalSearchBtn = document.getElementById('generalSearchBtn');
            const residentSearchApartment = document.getElementById('residentSearchApartment');
            const apartmentSearchBtn = document.getElementById('apartmentSearchBtn');
            const residentSearchBlock = document.getElementById('residentSearchBlock');
            const residentSearchSubBlock = document.getElementById('residentSearchSubBlock');
            
            // Plaka arama
            if (licensePlateSearch) {
                licensePlateSearch.addEventListener('input', (e) => {
                    this.clearOtherSearchFields('licensePlate');
                    e.target.value = e.target.value.toUpperCase();
                    this.currentPage = 1; // Reset to first page
                    if (e.target.value.length >= 2) {
                        this.searchByLicensePlate(e.target.value);
                    } else if (e.target.value.length === 0) {
                        this.loadResidents();
                    }
                });
                
                licensePlateSearch.addEventListener('keypress', (e) => {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        const plate = e.target.value.trim();
                        if (plate) {
                            this.searchByLicensePlate(plate);
                        } else {
                            this.loadResidents();
                        }
                    }
                });
            }
            
            if (searchByPlateBtn) {
                searchByPlateBtn.addEventListener('click', () => {
                    const plate = licensePlateSearch.value.trim();
                    if (plate) {
                        this.searchByLicensePlate(plate);
                    } else {
                        this.loadResidents();
                    }
                });
            }

            // Genel arama
            if (residentSearch) {
                residentSearch.addEventListener('input', (e) => {
                    this.clearOtherSearchFields('general');
                    this.currentPage = 1; // Reset to first page
                    if (e.target.value.length >= 2) {
                        this.searchResidentsByGeneral(e.target.value);
                    } else if (e.target.value.length === 0) {
                        this.loadResidents();
                    }
                });
                
                residentSearch.addEventListener('keypress', (e) => {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        const term = e.target.value.trim();
                        if (term) {
                            this.searchResidentsByGeneral(term);
                        } else {
                            this.loadResidents();
                        }
                    }
                });
            }
            
            if (generalSearchBtn) {
                generalSearchBtn.addEventListener('click', () => {
                    const term = residentSearch.value.trim();
                    if (term) {
                        this.searchResidentsByGeneral(term);
                    } else {
                        this.loadResidents();
                    }
                });
            }

            // Daire no arama
            if (residentSearchApartment) {
                residentSearchApartment.addEventListener('input', (e) => {
                    this.clearOtherSearchFields('apartment');
                    this.currentPage = 1; // Reset to first page
                    if (e.target.value.length >= 1) {
                        this.searchResidentsByApartment(e.target.value);
                    } else if (e.target.value.length === 0) {
                        this.loadResidents();
                    }
                });
                
                residentSearchApartment.addEventListener('keypress', (e) => {
                    if (e.key === 'Enter') {
                        e.preventDefault();
                        const apt = e.target.value.trim();
                        if (apt) {
                            this.searchResidentsByApartment(apt);
                        } else {
                            this.loadResidents();
                        }
                    }
                });
            }
            
            if (apartmentSearchBtn) {
                apartmentSearchBtn.addEventListener('click', () => {
                    const apt = residentSearchApartment.value.trim();
                    if (apt) {
                        this.searchResidentsByApartment(apt);
                    } else {
                        this.loadResidents();
                    }
                });
            }

            // Ana blok arama
            if (residentSearchBlock) {
                residentSearchBlock.addEventListener('change', (e) => {
                    this.clearOtherSearchFields('block');
                    this.currentPage = 1; // Reset to first page
                    this.searchResidentsByBlock();
                });
            }

            // Alt blok arama
            if (residentSearchSubBlock) {
                residentSearchSubBlock.addEventListener('change', (e) => {
                    this.clearOtherSearchFields('subBlock');
                    this.currentPage = 1; // Reset to first page
                    this.searchResidentsByBlock();
                });
            }

            // Sayfa boyutu değişikliği
            const residentsPageSize = document.getElementById('residentsPageSize');
            if (residentsPageSize) {
                residentsPageSize.addEventListener('change', (e) => {
                    this.pageSize = parseInt(e.target.value);
                    this.currentPage = 1;
                    this.refreshCurrentView();
                });
            }

            const saveResident = document.getElementById('saveResident');
            if (saveResident) {
                saveResident.addEventListener('click', () => this.saveResident());
            }

            const exportResidentsBtn = document.getElementById('exportResidentsBtn');
            if (exportResidentsBtn) {
                exportResidentsBtn.addEventListener('click', () => this.exportResidents());
            }

            const importResidentsBtn = document.getElementById('importResidentsBtn');
            if (importResidentsBtn) {
                importResidentsBtn.addEventListener('click', () => this.showImportModal());
            }

            const importResidents = document.getElementById('importResidents');
            if (importResidents) {
                importResidents.addEventListener('click', () => this.importResidents());
            }

            const addContactBtn = document.getElementById('addContactBtn');
            if (addContactBtn) {
                addContactBtn.addEventListener('click', () => this.addContactRow());
            }

            const addVehicleBtn = document.getElementById('addVehicleBtn');
            if (addVehicleBtn) {
                addVehicleBtn.addEventListener('click', () => this.addVehicleRow());
            }

            // Otomatik daire numarası oluşturma
            const residentBlock = document.getElementById('residentBlock');
            const residentSubBlock = document.getElementById('residentSubBlock');
            const residentDoorNumber = document.getElementById('residentDoorNumber');
            
            if (residentBlock && residentSubBlock && residentDoorNumber) {
                residentBlock.addEventListener('change', () => this.updateApartmentNumber());
                residentSubBlock.addEventListener('change', () => this.updateApartmentNumber());
                residentDoorNumber.addEventListener('change', () => this.updateApartmentNumber());
            }
        }

        // Users tab events (Admin only)
        if (this.user.role === 'Admin') {
            const addUserBtn = document.getElementById('addUserBtn');
            if (addUserBtn) {
                addUserBtn.addEventListener('click', () => this.showUserModal());
            }

            const saveUser = document.getElementById('saveUser');
            if (saveUser) {
                saveUser.addEventListener('click', () => this.saveUser());
            }

            // Mail Settings events
            const mailSettingsForm = document.getElementById('mailSettingsForm');
            if (mailSettingsForm) {
                mailSettingsForm.addEventListener('submit', (e) => this.saveMailSettings(e));
            }

            const testConnectionBtn = document.getElementById('testConnectionBtn');
            if (testConnectionBtn) {
                testConnectionBtn.addEventListener('click', () => this.testMailConnection());
            }

            const sendTestMailBtn = document.getElementById('sendTestMailBtn');
            if (sendTestMailBtn) {
                sendTestMailBtn.addEventListener('click', () => this.showTestMailModal());
            }

            const sendTestMailConfirm = document.getElementById('sendTestMailConfirm');
            if (sendTestMailConfirm) {
                sendTestMailConfirm.addEventListener('click', () => this.sendTestMail());
            }

            const loadMailSettingsBtn = document.getElementById('loadMailSettingsBtn');
            if (loadMailSettingsBtn) {
                loadMailSettingsBtn.addEventListener('click', () => this.loadMailSettings());
            }

            const deactivateMailBtn = document.getElementById('deactivateMailBtn');
            if (deactivateMailBtn) {
                deactivateMailBtn.addEventListener('click', () => this.deactivateMailSettings());
            }

            const togglePassword = document.getElementById('togglePassword');
            if (togglePassword) {
                togglePassword.addEventListener('click', () => this.togglePasswordVisibility());
            }
        }

        // Set default dates
        this.setDefaultDates();
        this.setStatsDefaultDates();
    }

    setDefaultDates() {
        const today = new Date();
        const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
        
        const reportStartDate = document.getElementById('reportStartDate');
        const reportEndDate = document.getElementById('reportEndDate');
        const filterDate = document.getElementById('filterDate');

        if (reportStartDate) {
            reportStartDate.value = firstDayOfMonth.toISOString().split('T')[0];
        }
        if (reportEndDate) {
            reportEndDate.value = today.toISOString().split('T')[0];
        }
        if (filterDate) {
            filterDate.value = today.toISOString().split('T')[0];
        }
    }

    setStatsDefaultDates() {
        const today = new Date();
        const firstDayOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);
        
        const statsStartDate = document.getElementById('statsStartDate');
        const statsEndDate = document.getElementById('statsEndDate');

        if (statsStartDate) {
            statsStartDate.value = firstDayOfMonth.toISOString().split('T')[0];
        }
        if (statsEndDate) {
            statsEndDate.value = today.toISOString().split('T')[0];
        }
    }

    loadUserInfo() {
        const userInfo = document.getElementById('userInfo');
        if (userInfo && this.user) {
            userInfo.textContent = `${this.user.fullName} (${this.getRoleDisplayName(this.user.role)})`;
        }
    }

    async loadInitialData() {
        try {
            await Promise.all([
                this.loadVisitors(),
                this.loadStatistics(),
                this.loadApartmentStats(),
                this.user.role === 'Admin' ? this.loadUsers() : Promise.resolve(),
                this.user.role === 'Admin' ? this.loadResidents() : Promise.resolve(),
                this.user.role === 'Admin' ? this.loadMailSettings() : Promise.resolve(),
                this.user.role === 'Admin' ? this.loadSmtpPresets() : Promise.resolve()
            ]);
        } catch (error) {
            console.error('Load initial data error:', error);
            this.showError('Veri yükleme hatası');
        }
    }

    logout() {
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
        this.showSuccess('Çıkış yapıldı');
        
        setTimeout(() => {
            window.location.href = 'login.html';
        }, 1000);
    }

    async loadVisitors() {
        try {
            this.visitorCurrentSearchMode = 'all';
            this.visitorLastSearchData = null;
            
            const response = await this.apiCall(`/visitor/paged?page=${this.visitorCurrentPage}&pageSize=${this.visitorPageSize}`);
            
            if (response.ok) {
                const pagedData = await response.json();
                this.visitorTotalRecords = pagedData.totalCount;
                this.displayVisitors(pagedData.visitors);
                this.displayVisitorPagination(pagedData);
                this.displayVisitorInfo(pagedData);
            } else {
                this.showError('Ziyaretçi listesi yüklenemedi');
            }
        } catch (error) {
            console.error('Load visitors error:', error);
            this.showError('Veriler yüklenirken hata oluştu');
        }
    }

    async searchVisitors() {
        try {
            this.visitorCurrentSearchMode = 'search';
            
            const searchData = {
                date: document.getElementById('filterDate')?.value || '',
                apartmentNumber: document.getElementById('filterApartment')?.value || '',
                licensePlate: document.getElementById('filterPlate')?.value || '',
                status: document.getElementById('filterStatus')?.value || '',
                page: this.visitorCurrentPage,
                pageSize: this.visitorPageSize
            };
            
            this.visitorLastSearchData = searchData;

            const response = await this.apiCall('/visitor/search', 'POST', searchData);
            
            if (response.ok) {
                const pagedData = await response.json();
                this.visitorTotalRecords = pagedData.totalCount;
                this.displayVisitors(pagedData.visitors);
                this.displayVisitorPagination(pagedData);
                this.displayVisitorInfo(pagedData);
            } else {
                console.log('Visitor search failed');
            }
        } catch (error) {
            console.error('Search visitors error:', error);
        }
    }

    async refreshVisitorCurrentView() {
        switch (this.visitorCurrentSearchMode) {
            case 'all':
                await this.loadVisitors();
                break;
            case 'search':
                await this.searchVisitors();
                break;
        }
    }

    displayVisitors(visitors) {
        const tbody = document.getElementById('visitorsTableBody');
        if (!tbody) return;
        
        if (visitors.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center">Ziyaretçi kaydı bulunamadı</td></tr>';
            return;
        }

        tbody.innerHTML = visitors.map(visitor => `
            <tr>
                <td>
                    <strong>${this.escapeHtml(visitor.fullName)}</strong>
                    ${visitor.idNumber ? `<br><small class="text-muted">TC: ${this.escapeHtml(visitor.idNumber)}</small>` : ''}
                    ${visitor.residentName ? `<br><small class="text-muted">Daire Sahibi: ${this.escapeHtml(visitor.residentName)}</small>` : ''}
                    ${visitor.visitorPhone ? `<br><small class="text-muted">Ziyaretçi Tel: ${this.escapeHtml(visitor.visitorPhone)}</small>` : ''}
                </td>
                <td>
                    <a href="#" class="text-decoration-none" onclick="admin.filterByApartment('${this.escapeHtml(visitor.apartmentNumber)}'); return false;">
                        ${this.escapeHtml(visitor.apartmentNumber)}
                    </a>
                </td>
                <td>
                    ${visitor.licensePlate ? 
                        `<a href="#" class="text-decoration-none" onclick="admin.filterByPlate('${this.escapeHtml(visitor.licensePlate)}'); return false;">
                            ${this.escapeHtml(visitor.licensePlate)}
                        </a>` 
                        : '-'}
                </td>
                <td>${this.formatDateTime(visitor.checkInTime)}</td>
                <td>${visitor.checkOutTime ? this.formatDateTime(visitor.checkOutTime) : '-'}</td>
                <td>
                    <span class="badge ${visitor.isActive ? 'bg-success' : 'bg-secondary'}">
                        ${visitor.isActive ? 'Aktif' : 'Çıkış Yapılmış'}
                    </span>
                </td>
                <td>
                    ${visitor.isActive ? `
                        <button class="btn btn-sm btn-warning me-1" onclick="admin.checkoutVisitor(${visitor.id})">
                            <i class="bi bi-box-arrow-right"></i>
                        </button>
                    ` : ''}
                    ${visitor.photoPath ? `
                        <button class="btn btn-sm btn-info me-1" onclick="admin.viewPhoto('${this.escapeHtml(visitor.photoPath)}')">
                            <i class="bi bi-image"></i>
                        </button>
                    ` : ''}
                    ${this.user.role === 'Admin' ? `
                        <button class="btn btn-sm btn-danger" onclick="admin.deleteVisitor(${visitor.id})">
                            <i class="bi bi-trash"></i>
                        </button>
                    ` : ''}
                </td>
            </tr>
        `).join('');
    }

    displayVisitorPagination(pagedData) {
        const paginationContainer = document.getElementById('visitorsPagination');
        if (!paginationContainer) return;

        const { pageNumber, totalPages, hasPreviousPage, hasNextPage } = pagedData;
        
        let paginationHtml = '';

        // Previous button
        if (hasPreviousPage) {
            paginationHtml += `<li class="page-item">
                <button class="page-link" onclick="admin.goToVisitorPage(${pageNumber - 1})">Önceki</button>
            </li>`;
        } else {
            paginationHtml += `<li class="page-item disabled">
                <span class="page-link">Önceki</span>
            </li>`;
        }

        // Page numbers
        const startPage = Math.max(1, pageNumber - 2);
        const endPage = Math.min(totalPages, pageNumber + 2);

        if (startPage > 1) {
            paginationHtml += `<li class="page-item">
                <button class="page-link" onclick="admin.goToVisitorPage(1)">1</button>
            </li>`;
            if (startPage > 2) {
                paginationHtml += `<li class="page-item disabled">
                    <span class="page-link">...</span>
                </li>`;
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            if (i === pageNumber) {
                paginationHtml += `<li class="page-item active">
                    <span class="page-link">${i}</span>
                </li>`;
            } else {
                paginationHtml += `<li class="page-item">
                    <button class="page-link" onclick="admin.goToVisitorPage(${i})">${i}</button>
                </li>`;
            }
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                paginationHtml += `<li class="page-item disabled">
                    <span class="page-link">...</span>
                </li>`;
            }
            paginationHtml += `<li class="page-item">
                <button class="page-link" onclick="admin.goToVisitorPage(${totalPages})">${totalPages}</button>
            </li>`;
        }

        // Next button
        if (hasNextPage) {
            paginationHtml += `<li class="page-item">
                <button class="page-link" onclick="admin.goToVisitorPage(${pageNumber + 1})">Sonraki</button>
            </li>`;
        } else {
            paginationHtml += `<li class="page-item disabled">
                <span class="page-link">Sonraki</span>
            </li>`;
        }

        paginationContainer.innerHTML = paginationHtml;
    }

    displayVisitorInfo(pagedData) {
        const infoContainer = document.getElementById('visitorsInfo');
        if (!infoContainer) return;

        const { pageNumber, pageSize, totalCount } = pagedData;
        const startRecord = (pageNumber - 1) * pageSize + 1;
        const endRecord = Math.min(pageNumber * pageSize, totalCount);

        infoContainer.innerHTML = `${startRecord}-${endRecord} / ${totalCount} kayıt`;
    }

    async goToVisitorPage(page) {
        if (page < 1 || page > Math.ceil(this.visitorTotalRecords / this.visitorPageSize)) return;
        
        this.visitorCurrentPage = page;
        await this.refreshVisitorCurrentView();
    }

    filterByApartment(apartmentNumber) {
        document.getElementById('filterDate').value = '';
        document.getElementById('filterApartment').value = apartmentNumber;
        document.getElementById('filterPlate').value = '';
        document.getElementById('filterStatus').value = '';
        this.visitorCurrentPage = 1;
        this.searchVisitors();
    }

    filterByPlate(licensePlate) {
        document.getElementById('filterDate').value = '';
        document.getElementById('filterApartment').value = '';
        document.getElementById('filterPlate').value = licensePlate;
        document.getElementById('filterStatus').value = '';
        this.visitorCurrentPage = 1;
        this.searchVisitors();
    }

    async applyFilters() {
        const date = document.getElementById('filterDate')?.value;
        const apartment = document.getElementById('filterApartment')?.value;
        const status = document.getElementById('filterStatus')?.value;

        try {
            let url = '/visitor';
            const params = new URLSearchParams();

            if (date) {
                const startDate = new Date(date);
                const endDate = new Date(date);
                endDate.setDate(endDate.getDate() + 1);
                
                url = '/visitor/daterange';
                params.append('startDate', startDate.toISOString());
                params.append('endDate', endDate.toISOString());
            }

            if (apartment) {
                url = `/visitor/apartment/${encodeURIComponent(apartment)}`;
            }

            const response = await this.apiCall(`${url}?${params}`);

            if (response.ok) {
                let visitors = await response.json();
                
                // Apply status filter
                if (status) {
                    visitors = visitors.filter(v => 
                        status === 'active' ? v.isActive : !v.isActive
                    );
                }
                
                this.displayVisitors(visitors);
            }
        } catch (error) {
            console.error('Filter error:', error);
            this.showError('Filtreleme sırasında hata oluştu');
        }
    }

    async checkoutVisitor(visitorId) {
        if (!confirm('Bu ziyaretçiyi çıkış yaptırmak istediğinizden emin misiniz?')) {
            return;
        }

        try {
            const response = await this.apiCall(`/visitor/${visitorId}/checkout`, 'POST');

            if (response.ok) {
                this.showSuccess('Ziyaretçi çıkışı başarıyla kaydedildi');
                await Promise.all([
                    this.loadVisitors(),
                    this.loadStatistics()
                ]);
            } else {
                this.showError('Çıkış işlemi başarısız');
            }
        } catch (error) {
            console.error('Checkout error:', error);
            this.showError('Çıkış işlemi sırasında hata oluştu');
        }
    }

    async deleteVisitor(visitorId) {
        if (!confirm('Bu ziyaretçi kaydını silmek istediğinizden emin misiniz?')) {
            return;
        }

        try {
            const response = await this.apiCall(`/visitor/${visitorId}`, 'DELETE');

            if (response.ok) {
                this.showSuccess('Ziyaretçi kaydı silindi');
                await Promise.all([
                    this.loadVisitors(),
                    this.loadStatistics()
                ]);
            } else {
                this.showError('Silme işlemi başarısız');
            }
        } catch (error) {
            console.error('Delete error:', error);
            this.showError('Silme işlemi sırasında hata oluştu');
        }
    }

    viewPhoto(photoPath) {
        window.open(photoPath, '_blank');
    }

    async loadStatistics() {
        try {
            const today = new Date();
            const startOfToday = new Date(today.getFullYear(), today.getMonth(), today.getDate());
            const startOfWeek = new Date(today.setDate(today.getDate() - today.getDay()));
            const startOfMonth = new Date(today.getFullYear(), today.getMonth(), 1);

            const requests = [
                this.apiCall(`/visitor/daterange?startDate=${startOfToday.toISOString()}&endDate=${new Date().toISOString()}`),
                this.apiCall('/visitor/active'),
                this.apiCall(`/visitor/daterange?startDate=${startOfWeek.toISOString()}&endDate=${new Date().toISOString()}`),
                this.apiCall(`/visitor/daterange?startDate=${startOfMonth.toISOString()}&endDate=${new Date().toISOString()}`)
            ];

            const [todayResponse, activeResponse, weekResponse, monthResponse] = await Promise.all(requests);

            if (todayResponse.ok) {
                const todayVisitors = await todayResponse.json();
                this.updateElement('todayVisitors', todayVisitors.length);
            }

            if (activeResponse.ok) {
                const activeVisitors = await activeResponse.json();
                this.updateElement('activeVisitors', activeVisitors.length);
            }

            if (weekResponse.ok) {
                const weekVisitors = await weekResponse.json();
                this.updateElement('weekVisitors', weekVisitors.length);
            }

            if (monthResponse.ok) {
                const monthVisitors = await monthResponse.json();
                this.updateElement('monthVisitors', monthVisitors.length);
            }

        } catch (error) {
            console.error('Load statistics error:', error);
        }
    }

    async generateReport() {
        const startDate = document.getElementById('reportStartDate')?.value;
        const endDate = document.getElementById('reportEndDate')?.value;

        if (!startDate || !endDate) {
            this.showError('Başlangıç ve bitiş tarihleri gerekli');
            return;
        }

        try {
            console.log('Generating report for:', startDate, 'to', endDate);
            const url = `/visitor/export?startDate=${startDate}&endDate=${endDate}`;
            console.log('Request URL:', `${this.apiBase}${url}`);
            
            const response = await fetch(`${this.apiBase}${url}`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${this.token}`
                }
            });

            console.log('Response status:', response.status);
            console.log('Response headers:', response.headers);

            if (response.ok) {
                const blob = await response.blob();
                console.log('Blob size:', blob.size);
                
                if (blob.size === 0) {
                    this.showError('Rapor dosyası boş');
                    return;
                }
                
                const downloadUrl = window.URL.createObjectURL(blob);
                const link = document.createElement('a');
                link.href = downloadUrl;
                link.download = `ziyaretci_raporu_${startDate}_${endDate}.xlsx`;
                document.body.appendChild(link);
                link.click();
                document.body.removeChild(link);
                window.URL.revokeObjectURL(downloadUrl);
                
                this.showSuccess('Excel raporu indirildi');
            } else {
                const errorText = await response.text();
                console.error('Report generation failed:', errorText);
                this.showError(`Rapor oluşturulamadı: ${errorText}`);
            }
        } catch (error) {
            console.error('Generate report error:', error);
            this.showError('Rapor oluşturma sırasında hata oluştu');
        }
    }

    async loadApartmentStats() {
        try {
            const startDate = document.getElementById('statsStartDate')?.value;
            const endDate = document.getElementById('statsEndDate')?.value;
            
            let url = '/visitor/apartment-stats';
            if (startDate && endDate) {
                url += `?startDate=${startDate}&endDate=${endDate}`;
            }
            
            console.log('Loading apartment stats with URL:', url);
            const response = await this.apiCall(url);
            
            console.log('Apartment stats response status:', response.status);
            
            if (response.ok) {
                const stats = await response.json();
                console.log('Apartment stats data:', stats);
                this.displayApartmentStats(stats);
            } else {
                const errorText = await response.text();
                console.error('Apartment stats failed:', errorText);
                this.showError(`Daire istatistikleri yüklenemedi: ${errorText}`);
            }
        } catch (error) {
            console.error('Load apartment stats error:', error);
            this.showError('Daire istatistikleri yüklenirken hata oluştu');
        }
    }

    displayApartmentStats(stats) {
        const tbody = document.getElementById('apartmentStatsTableBody');
        if (!tbody) return;

        if (stats.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center">Veri bulunamadı</td></tr>';
            return;
        }

        tbody.innerHTML = stats.map((stat, index) => `
            <tr>
                <td>
                    <span class="badge bg-primary">${index + 1}</span>
                </td>
                <td>
                    <strong>${this.escapeHtml(stat.apartmentNumber)}</strong>
                </td>
                <td>
                    <span class="badge bg-info">${stat.visitorCount}</span>
                </td>
                <td>
                    <span class="badge bg-success">${stat.activeVisitorCount}</span>
                </td>
                <td>
                    <small>${this.formatDateTime(stat.lastVisitDate)}</small>
                </td>
                <td>
                    <small>${this.escapeHtml(stat.mostFrequentVisitor)}</small>
                </td>
            </tr>
        `).join('');
    }

    async loadUsers() {
        if (this.user.role !== 'Admin') return;

        try {
            const response = await this.apiCall('/user');

            if (response.ok) {
                const users = await response.json();
                this.displayUsers(users);
            }
        } catch (error) {
            console.error('Load users error:', error);
        }
    }

    displayUsers(users) {
        const tbody = document.getElementById('usersTableBody');
        if (!tbody) return;
        
        if (users.length === 0) {
            tbody.innerHTML = '<tr><td colspan="6" class="text-center">Kullanıcı kaydı bulunamadı</td></tr>';
            return;
        }

        tbody.innerHTML = users.map(user => `
            <tr>
                <td>${this.escapeHtml(user.username)}</td>
                <td>${this.escapeHtml(user.fullName)}</td>
                <td>
                    <span class="badge ${this.getRoleBadgeClass(user.role)}">
                        ${this.getRoleDisplayName(user.role)}
                    </span>
                </td>
                <td>${user.lastLogin ? this.formatDateTime(user.lastLogin) : '-'}</td>
                <td>
                    <span class="badge ${user.isActive ? 'bg-success' : 'bg-secondary'}">
                        ${user.isActive ? 'Aktif' : 'Pasif'}
                    </span>
                </td>
                <td>
                    <button class="btn btn-sm btn-primary me-1" onclick="admin.editUser(${user.id})">
                        <i class="bi bi-pencil"></i>
                    </button>
                    ${user.id !== this.user.id ? `
                        <button class="btn btn-sm btn-danger" onclick="admin.deleteUser(${user.id})">
                            <i class="bi bi-trash"></i>
                        </button>
                    ` : ''}
                </td>
            </tr>
        `).join('');
    }

    // User management methods
    showUserModal(user = null) {
        const modal = new bootstrap.Modal(document.getElementById('userModal'));
        const form = document.getElementById('userForm');
        
        if (user) {
            document.getElementById('userModalTitle').textContent = 'Kullanıcı Düzenle';
            document.getElementById('userId').value = user.id;
            document.getElementById('userUsername').value = user.username;
            document.getElementById('userFullName').value = user.fullName;
            document.getElementById('userRole').value = user.role;
            document.getElementById('passwordField').style.display = 'none';
            document.getElementById('userPassword').required = false;
        } else {
            document.getElementById('userModalTitle').textContent = 'Yeni Kullanıcı';
            form.reset();
            document.getElementById('passwordField').style.display = 'block';
            document.getElementById('userPassword').required = true;
        }
        
        modal.show();
    }

    async saveUser() {
        const form = document.getElementById('userForm');
        
        if (!form.checkValidity()) {
            form.reportValidity();
            return;
        }

        const userData = {
            username: document.getElementById('userUsername').value,
            fullName: document.getElementById('userFullName').value,
            role: document.getElementById('userRole').value
        };

        const userId = document.getElementById('userId').value;
        const password = document.getElementById('userPassword').value;

        try {
            let response;
            
            if (userId) {
                response = await this.apiCall(`/user/${userId}`, 'PUT', userData);
            } else {
                response = await this.apiCall('/user', 'POST', { ...userData, password });
            }

            if (response.ok) {
                bootstrap.Modal.getInstance(document.getElementById('userModal')).hide();
                this.showSuccess(userId ? 'Kullanıcı güncellendi' : 'Kullanıcı oluşturuldu');
                this.loadUsers();
            } else {
                this.showError('Kullanıcı işlemi başarısız');
            }
        } catch (error) {
            console.error('Save user error:', error);
            this.showError('Kullanıcı kaydı sırasında hata oluştu');
        }
    }

    async editUser(userId) {
        try {
            const response = await this.apiCall(`/user/${userId}`);

            if (response.ok) {
                const user = await response.json();
                this.showUserModal(user);
            }
        } catch (error) {
            console.error('Edit user error:', error);
        }
    }

    async deleteUser(userId) {
        if (!confirm('Bu kullanıcıyı silmek istediğinizden emin misiniz?')) {
            return;
        }

        try {
            const response = await this.apiCall(`/user/${userId}`, 'DELETE');

            if (response.ok) {
                this.showSuccess('Kullanıcı silindi');
                this.loadUsers();
            } else {
                this.showError('Kullanıcı silme işlemi başarısız');
            }
        } catch (error) {
            console.error('Delete user error:', error);
            this.showError('Kullanıcı silme sırasında hata oluştu');
        }
    }

    // Utility methods
    async apiCall(endpoint, method = 'GET', body = null) {
        const options = {
            method,
            headers: {
                'Authorization': `Bearer ${this.token}`,
                'Content-Type': 'application/json'
            }
        };

        if (body) {
            options.body = JSON.stringify(body);
        }

        return fetch(`${this.apiBase}${endpoint}`, options);
    }

    updateElement(id, value) {
        const element = document.getElementById(id);
        if (element) {
            element.textContent = value;
        }
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

    getRoleBadgeClass(role) {
        switch (role) {
            case 'Admin': return 'bg-danger';
            case 'Manager': return 'bg-warning';
            case 'Security': return 'bg-info';
            default: return 'bg-secondary';
        }
    }

    // Otomatik daire numarası oluşturma
    updateApartmentNumber() {
        const block = document.getElementById('residentBlock').value;
        const subBlock = document.getElementById('residentSubBlock').value;
        const doorNumber = document.getElementById('residentDoorNumber').value;
        
        if (block && subBlock && doorNumber) {
            const apartmentNumber = `${block}${subBlock}-${doorNumber}`;
            document.getElementById('residentApartmentNumber').value = apartmentNumber;
        } else {
            document.getElementById('residentApartmentNumber').value = '';
        }
    }

    // Sayfa yenileme
    async refreshCurrentView() {
        switch (this.currentSearchMode) {
            case 'all':
                await this.loadResidents();
                break;
            case 'search':
                await this.searchResidentsAsync(this.lastSearchData);
                break;
            case 'licensePlate':
                await this.searchByLicensePlate(this.lastSearchData);
                break;
            case 'general':
                await this.searchResidentsByGeneral(this.lastSearchData);
                break;
            case 'apartment':
                await this.searchResidentsByApartment(this.lastSearchData);
                break;
            case 'block':
                await this.searchResidentsByBlock();
                break;
        }
    }

    // Resident Management Methods
    async loadResidents() {
        try {
            this.currentSearchMode = 'all';
            this.lastSearchData = null;
            
            const response = await this.apiCall(`/resident/paged?page=${this.currentPage}&pageSize=${this.pageSize}`);
            
            if (response.ok) {
                const pagedData = await response.json();
                this.totalRecords = pagedData.totalCount;
                this.displayResidents(pagedData.residents);
                this.displayPagination(pagedData);
                this.displayInfo(pagedData);
            } else {
                this.showError('Daire sahipleri listesi yüklenemedi');
            }
        } catch (error) {
            console.error('Load residents error:', error);
            this.showError('Daire sahipleri yüklenirken hata oluştu');
        }
    }

    displayResidents(residents) {
        const tbody = document.getElementById('residentsTableBody');
        if (!tbody) return;

        if (residents.length === 0) {
            tbody.innerHTML = '<tr><td colspan="7" class="text-center">Kayıtlı daire sahibi bulunamadı</td></tr>';
            return;
        }

        tbody.innerHTML = residents.map(resident => `
            <tr>
                <td>${resident.fullName}</td>
                <td>${resident.apartmentNumber}</td>
                <td>${resident.block || ''}</td>
                <td>
                    ${resident.contacts.length > 0 ? 
                        resident.contacts.slice(0, 2).map(c => `${c.contactValue} (${c.contactType})`).join('<br>') +
                        (resident.contacts.length > 2 ? '<br>...' : '')
                        : 'Bilgi yok'}
                </td>
                <td>${resident.vehicles.length}</td>
                <td>${this.formatDateTime(resident.createdAt)}</td>
                <td>
                    <button class="btn btn-sm btn-primary" onclick="admin.editResident(${resident.id})">
                        <i class="bi bi-pencil"></i>
                    </button>
                    <button class="btn btn-sm btn-danger" onclick="admin.deleteResident(${resident.id})">
                        <i class="bi bi-trash"></i>
                    </button>
                </td>
            </tr>
        `).join('');
    }

    async searchResidents() {
        try {
            const searchTerm = document.getElementById('residentSearch').value;
            const block = document.getElementById('residentSearchBlock').value;
            const subBlock = document.getElementById('residentSearchSubBlock').value;
            const apartmentNumber = document.getElementById('residentSearchApartment').value;

            const searchData = {
                searchTerm,
                block,
                subBlock,
                apartmentNumber,
                page: 1,
                pageSize: 50
            };

            const response = await this.apiCall('/resident/search', 'POST', searchData);
            
            if (response.ok) {
                const residents = await response.json();
                this.displayResidents(residents);
            } else {
                this.showError('Arama yapılırken hata oluştu');
            }
        } catch (error) {
            console.error('Search residents error:', error);
            this.showError('Arama yapılırken hata oluştu');
        }
    }

    // Diğer arama alanlarını sıfırlama
    clearOtherSearchFields(activeField) {
        const fields = {
            licensePlate: 'licensePlateSearch',
            general: 'residentSearch',
            apartment: 'residentSearchApartment',
            block: 'residentSearchBlock',
            subBlock: 'residentSearchSubBlock'
        };

        Object.keys(fields).forEach(key => {
            if (key !== activeField) {
                const element = document.getElementById(fields[key]);
                if (element) {
                    element.value = '';
                }
            }
        });
    }

    async searchByLicensePlate(licensePlate) {
        try {
            console.log('Searching for license plate:', licensePlate);
            const response = await this.apiCall(`/resident/search/license/${encodeURIComponent(licensePlate)}`);
            
            console.log('Response status:', response.status);
            
            if (response.ok) {
                const residents = await response.json();
                console.log('Found residents:', residents);
                this.displayResidents(residents);
            } else {
                const errorText = await response.text();
                console.log('License plate search failed:', errorText);
            }
        } catch (error) {
            console.error('License plate search error:', error);
        }
    }

    async searchResidentsByGeneral(searchTerm) {
        try {
            console.log('General search:', searchTerm);
            this.currentSearchMode = 'general';
            this.lastSearchData = searchTerm;
            
            const searchData = {
                searchTerm: searchTerm.trim(),
                block: '',
                subBlock: '',
                apartmentNumber: '',
                page: this.currentPage,
                pageSize: this.pageSize
            };

            const response = await this.apiCall('/resident/search', 'POST', searchData);
            
            if (response.ok) {
                const pagedData = await response.json();
                console.log('Found residents:', pagedData);
                this.totalRecords = pagedData.totalCount;
                this.displayResidents(pagedData.residents);
                this.displayPagination(pagedData);
                this.displayInfo(pagedData);
            } else {
                console.log('General search failed');
            }
        } catch (error) {
            console.error('General search error:', error);
        }
    }

    async searchResidentsByApartment(apartmentNumber) {
        try {
            console.log('Apartment search:', apartmentNumber);
            this.currentSearchMode = 'apartment';
            this.lastSearchData = apartmentNumber;
            
            const searchData = {
                searchTerm: '',
                block: '',
                subBlock: '',
                apartmentNumber: apartmentNumber.trim(),
                page: this.currentPage,
                pageSize: this.pageSize
            };

            const response = await this.apiCall('/resident/search', 'POST', searchData);
            
            if (response.ok) {
                const pagedData = await response.json();
                console.log('Found residents:', pagedData);
                this.totalRecords = pagedData.totalCount;
                this.displayResidents(pagedData.residents);
                this.displayPagination(pagedData);
                this.displayInfo(pagedData);
            } else {
                console.log('Apartment search failed');
            }
        } catch (error) {
            console.error('Apartment search error:', error);
        }
    }

    async searchResidentsByBlock() {
        try {
            const block = document.getElementById('residentSearchBlock').value;
            const subBlock = document.getElementById('residentSearchSubBlock').value;
            
            console.log('Block search - Block:', block, 'SubBlock:', subBlock);
            this.currentSearchMode = 'block';
            this.lastSearchData = { block, subBlock };
            
            const searchData = {
                searchTerm: '',
                block: block,
                subBlock: subBlock,
                apartmentNumber: '',
                page: this.currentPage,
                pageSize: this.pageSize
            };

            const response = await this.apiCall('/resident/search', 'POST', searchData);
            
            if (response.ok) {
                const pagedData = await response.json();
                console.log('Found residents:', pagedData);
                this.totalRecords = pagedData.totalCount;
                this.displayResidents(pagedData.residents);
                this.displayPagination(pagedData);
                this.displayInfo(pagedData);
            } else {
                console.log('Block search failed');
            }
        } catch (error) {
            console.error('Block search error:', error);
        }
    }

    displayPagination(pagedData) {
        const paginationContainer = document.getElementById('residentsPagination');
        if (!paginationContainer) return;

        const { pageNumber, totalPages, hasPreviousPage, hasNextPage } = pagedData;
        
        let paginationHtml = '';

        // Previous button
        if (hasPreviousPage) {
            paginationHtml += `<li class="page-item">
                <button class="page-link" onclick="admin.goToPage(${pageNumber - 1})">Önceki</button>
            </li>`;
        } else {
            paginationHtml += `<li class="page-item disabled">
                <span class="page-link">Önceki</span>
            </li>`;
        }

        // Page numbers
        const startPage = Math.max(1, pageNumber - 2);
        const endPage = Math.min(totalPages, pageNumber + 2);

        if (startPage > 1) {
            paginationHtml += `<li class="page-item">
                <button class="page-link" onclick="admin.goToPage(1)">1</button>
            </li>`;
            if (startPage > 2) {
                paginationHtml += `<li class="page-item disabled">
                    <span class="page-link">...</span>
                </li>`;
            }
        }

        for (let i = startPage; i <= endPage; i++) {
            if (i === pageNumber) {
                paginationHtml += `<li class="page-item active">
                    <span class="page-link">${i}</span>
                </li>`;
            } else {
                paginationHtml += `<li class="page-item">
                    <button class="page-link" onclick="admin.goToPage(${i})">${i}</button>
                </li>`;
            }
        }

        if (endPage < totalPages) {
            if (endPage < totalPages - 1) {
                paginationHtml += `<li class="page-item disabled">
                    <span class="page-link">...</span>
                </li>`;
            }
            paginationHtml += `<li class="page-item">
                <button class="page-link" onclick="admin.goToPage(${totalPages})">${totalPages}</button>
            </li>`;
        }

        // Next button
        if (hasNextPage) {
            paginationHtml += `<li class="page-item">
                <button class="page-link" onclick="admin.goToPage(${pageNumber + 1})">Sonraki</button>
            </li>`;
        } else {
            paginationHtml += `<li class="page-item disabled">
                <span class="page-link">Sonraki</span>
            </li>`;
        }

        paginationContainer.innerHTML = paginationHtml;
    }

    displayInfo(pagedData) {
        const infoContainer = document.getElementById('residentsInfo');
        if (!infoContainer) return;

        const { pageNumber, pageSize, totalCount } = pagedData;
        const startRecord = (pageNumber - 1) * pageSize + 1;
        const endRecord = Math.min(pageNumber * pageSize, totalCount);

        infoContainer.innerHTML = `${startRecord}-${endRecord} / ${totalCount} kayıt`;
    }

    async goToPage(page) {
        if (page < 1 || page > Math.ceil(this.totalRecords / this.pageSize)) return;
        
        this.currentPage = page;
        await this.refreshCurrentView();
    }

    showResidentModal(resident = null) {
        const modal = document.getElementById('residentModal');
        const title = document.getElementById('residentModalTitle');
        const form = document.getElementById('residentForm');
        
        if (resident) {
            title.textContent = 'Daire Sahibi Düzenle';
            this.fillResidentForm(resident);
        } else {
            title.textContent = 'Yeni Daire Sahibi Ekle';
            form.reset();
            document.getElementById('residentId').value = '';
            document.getElementById('contactsContainer').innerHTML = '';
            document.getElementById('vehiclesContainer').innerHTML = '';
        }
        
        const bsModal = new bootstrap.Modal(modal);
        bsModal.show();
    }

    fillResidentForm(resident) {
        document.getElementById('residentId').value = resident.id;
        document.getElementById('residentFullName').value = resident.fullName;
        
        // Ana blok ve alt blok bilgilerini düzgün şekilde ayarla
        // Öncelikle originalBlock ve originalSubBlock'u kullan, yoksa block'u ayır
        let blockValue = resident.originalBlock || '';
        let subBlockValue = resident.originalSubBlock || '';
        
        // Eğer original değerler yoksa ve block "A3" formatında geliyorsa, bunu "A" ve "3" olarak ayır
        if (!blockValue && resident.block && resident.block.length > 1) {
            const match = resident.block.match(/^([A-Z])(\d+)$/);
            if (match) {
                blockValue = match[1];     // A
                subBlockValue = match[2];  // 3
            } else {
                blockValue = resident.block;
            }
        }
        
        document.getElementById('residentBlock').value = blockValue;
        document.getElementById('residentSubBlock').value = subBlockValue;
        document.getElementById('residentDoorNumber').value = resident.doorNumber || '';
        document.getElementById('residentNotes').value = resident.notes || '';
        
        // Otomatik daire numarasını güncelle
        this.updateApartmentNumber();

        // Fill contacts
        const contactsContainer = document.getElementById('contactsContainer');
        contactsContainer.innerHTML = '';
        resident.contacts.forEach(contact => {
            this.addContactRow(contact);
        });

        // Fill vehicles
        const vehiclesContainer = document.getElementById('vehiclesContainer');
        vehiclesContainer.innerHTML = '';
        resident.vehicles.forEach(vehicle => {
            this.addVehicleRow(vehicle);
        });
    }

    addContactRow(contact = null) {
        const container = document.getElementById('contactsContainer');
        const index = container.children.length;
        
        const contactHtml = `
            <div class="contact-row mb-2" data-index="${index}">
                <div class="row">
                    <div class="col-md-3">
                        <select class="form-control" name="contactType_${index}">
                            <option value="Phone" ${contact?.contactType === 'Phone' ? 'selected' : ''}>Telefon</option>
                            <option value="Email" ${contact?.contactType === 'Email' ? 'selected' : ''}>E-posta</option>
                        </select>
                    </div>
                    <div class="col-md-4">
                        <input type="text" class="form-control" name="contactValue_${index}" placeholder="İletişim bilgisi" value="${contact?.contactValue || ''}">
                    </div>
                    <div class="col-md-2">
                        <input type="text" class="form-control" name="contactLabel_${index}" placeholder="Etiket" value="${contact?.label || ''}">
                    </div>
                    <div class="col-md-2">
                        <select class="form-control" name="contactPriority_${index}">
                            <option value="1" ${contact?.priority === 1 ? 'selected' : ''}>Yüksek</option>
                            <option value="2" ${contact?.priority === 2 ? 'selected' : ''}>Orta</option>
                            <option value="3" ${contact?.priority === 3 ? 'selected' : ''}>Düşük</option>
                        </select>
                    </div>
                    <div class="col-md-1">
                        <button type="button" class="btn btn-sm btn-danger" onclick="this.parentElement.parentElement.parentElement.remove()">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        container.insertAdjacentHTML('beforeend', contactHtml);
    }

    addVehicleRow(vehicle = null) {
        const container = document.getElementById('vehiclesContainer');
        const index = container.children.length;
        
        const vehicleHtml = `
            <div class="vehicle-row mb-2" data-index="${index}">
                <div class="row">
                    <div class="col-md-3">
                        <input type="text" class="form-control" name="vehiclePlate_${index}" placeholder="Plaka" value="${vehicle?.licensePlate || ''}">
                    </div>
                    <div class="col-md-2">
                        <input type="text" class="form-control" name="vehicleBrand_${index}" placeholder="Marka" value="${vehicle?.brand || ''}">
                    </div>
                    <div class="col-md-2">
                        <input type="text" class="form-control" name="vehicleModel_${index}" placeholder="Model" value="${vehicle?.model || ''}">
                    </div>
                    <div class="col-md-2">
                        <input type="text" class="form-control" name="vehicleColor_${index}" placeholder="Renk" value="${vehicle?.color || ''}">
                    </div>
                    <div class="col-md-2">
                        <input type="text" class="form-control" name="vehicleYear_${index}" placeholder="Yıl" value="${vehicle?.year || ''}">
                    </div>
                    <div class="col-md-1">
                        <button type="button" class="btn btn-sm btn-danger" onclick="this.parentElement.parentElement.parentElement.remove()">
                            <i class="bi bi-trash"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
        
        container.insertAdjacentHTML('beforeend', vehicleHtml);
    }

    async saveResident() {
        try {
            const residentId = document.getElementById('residentId').value;
            const fullName = document.getElementById('residentFullName').value;
            const block = document.getElementById('residentBlock').value;
            const subBlock = document.getElementById('residentSubBlock').value;
            const doorNumber = document.getElementById('residentDoorNumber').value;
            const notes = document.getElementById('residentNotes').value;

            if (!fullName || !block || !subBlock || !doorNumber) {
                this.showError('Ad soyad, Ana Blok, Alt Blok ve Kapı No zorunludur');
                return;
            }

            // Otomatik daire numarasını oluştur
            const apartmentNumber = `${block}${subBlock}-${doorNumber}`;

            // Collect contacts
            const contacts = [];
            const contactRows = document.querySelectorAll('.contact-row');
            contactRows.forEach(row => {
                const index = row.dataset.index;
                const contactType = row.querySelector(`[name="contactType_${index}"]`).value;
                const contactValue = row.querySelector(`[name="contactValue_${index}"]`).value;
                const label = row.querySelector(`[name="contactLabel_${index}"]`).value;
                const priority = parseInt(row.querySelector(`[name="contactPriority_${index}"]`).value);
                
                if (contactValue) {
                    contacts.push({ contactType, contactValue, label, priority });
                }
            });

            // Collect vehicles
            const vehicles = [];
            const vehicleRows = document.querySelectorAll('.vehicle-row');
            vehicleRows.forEach(row => {
                const index = row.dataset.index;
                const licensePlate = row.querySelector(`[name="vehiclePlate_${index}"]`).value;
                const brand = row.querySelector(`[name="vehicleBrand_${index}"]`).value;
                const model = row.querySelector(`[name="vehicleModel_${index}"]`).value;
                const color = row.querySelector(`[name="vehicleColor_${index}"]`).value;
                const year = row.querySelector(`[name="vehicleYear_${index}"]`).value;
                
                if (licensePlate) {
                    vehicles.push({ licensePlate, brand, model, color, year, vehicleType: 'Car' });
                }
            });

            const residentData = {
                fullName,
                apartmentNumber,
                block,
                subBlock,
                doorNumber,
                notes,
                contacts,
                vehicles
            };

            const url = residentId ? `/resident/${residentId}` : '/resident';
            const method = residentId ? 'PUT' : 'POST';

            const response = await this.apiCall(url, method, residentData);

            if (response.ok) {
                this.showSuccess(residentId ? 'Daire sahibi güncellendi' : 'Daire sahibi eklendi');
                const modal = bootstrap.Modal.getInstance(document.getElementById('residentModal'));
                modal.hide();
                this.loadResidents();
            } else {
                const error = await response.text();
                this.showError(error || 'Kayıt sırasında hata oluştu');
            }
        } catch (error) {
            console.error('Save resident error:', error);
            this.showError('Kayıt sırasında hata oluştu');
        }
    }

    async editResident(id) {
        try {
            const response = await this.apiCall(`/resident/${id}`);
            
            if (response.ok) {
                const resident = await response.json();
                this.showResidentModal(resident);
            } else {
                this.showError('Daire sahibi bilgileri yüklenemedi');
            }
        } catch (error) {
            console.error('Edit resident error:', error);
            this.showError('Daire sahibi düzenlenirken hata oluştu');
        }
    }

    async deleteResident(id) {
        if (!confirm('Bu daire sahibini silmek istediğinizden emin misiniz?')) {
            return;
        }

        try {
            const response = await this.apiCall(`/resident/${id}`, 'DELETE');
            
            if (response.ok) {
                this.showSuccess('Daire sahibi silindi');
                this.loadResidents();
            } else {
                this.showError('Silme işlemi sırasında hata oluştu');
            }
        } catch (error) {
            console.error('Delete resident error:', error);
            this.showError('Silme işlemi sırasında hata oluştu');
        }
    }

    showImportModal() {
        const modal = new bootstrap.Modal(document.getElementById('importModal'));
        modal.show();
    }

    async importResidents() {
        const fileInput = document.getElementById('importFile');
        const file = fileInput.files[0];
        
        if (!file) {
            this.showError('Lütfen bir dosya seçin');
            return;
        }

        try {
            const formData = new FormData();
            formData.append('file', file);

            const response = await fetch(`${this.apiBase}/resident/import`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${this.token}`
                },
                body: formData
            });

            if (response.ok) {
                const errors = await response.json();
                if (errors.length === 0) {
                    this.showSuccess('Tüm kayıtlar başarıyla içe aktarıldı');
                } else {
                    this.showError(`${errors.length} kayıtta hata oluştu`);
                }
                
                const modal = bootstrap.Modal.getInstance(document.getElementById('importModal'));
                modal.hide();
                this.loadResidents();
            } else {
                this.showError('İçe aktarma sırasında hata oluştu');
            }
        } catch (error) {
            console.error('Import residents error:', error);
            this.showError('İçe aktarma sırasında hata oluştu');
        }
    }

    async exportResidents() {
        try {
            const response = await fetch(`${this.apiBase}/resident/export`, {
                method: 'GET',
                headers: {
                    'Authorization': `Bearer ${this.token}`
                }
            });

            if (response.ok) {
                const blob = await response.blob();
                const url = window.URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `daire_sahipleri_${new Date().toISOString().split('T')[0]}.xlsx`;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                window.URL.revokeObjectURL(url);
                
                this.showSuccess('Rapor indirildi');
            } else {
                this.showError('Dışa aktarma sırasında hata oluştu');
            }
        } catch (error) {
            console.error('Export residents error:', error);
            this.showError('Dışa aktarma sırasında hata oluştu');
        }
    }

    getRoleDisplayName(role) {
        switch (role) {
            case 'Admin': return 'Sistem Yöneticisi';
            case 'Manager': return 'Yönetici';
            case 'Security': return 'Güvenlik';
            default: return role;
        }
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
        }
    }

    showError(message) {
        const toast = document.getElementById('errorToast');
        const body = document.getElementById('errorToastBody');
        
        if (toast && body) {
            body.textContent = message;
            const bsToast = new bootstrap.Toast(toast);
            bsToast.show();
        }
    }

    // Mail Settings Methods
    async loadMailSettings() {
        try {
            console.log('Loading mail settings, token:', this.token ? 'exists' : 'missing');
            
            const response = await this.apiCall('/mailsettings');
            
            if (response.ok) {
                const settings = await response.json();
                this.populateMailSettingsForm(settings);
            } else {
                console.error('Mail settings API error:', response.status, response.statusText);
                this.showError('Mail ayarları yüklenemedi');
            }
        } catch (error) {
            console.error('Error loading mail settings:', error);
            this.showError('Mail ayarları yüklenirken hata oluştu: ' + error.message);
        }
    }

    populateMailSettingsForm(settings) {
        document.getElementById('mailSenderName').value = settings.senderName || '';
        document.getElementById('mailSenderEmail').value = settings.senderEmail || '';
        document.getElementById('mailSmtpServer').value = settings.smtpServer || '';
        document.getElementById('mailPort').value = settings.port || 587;
        document.getElementById('mailUsername').value = settings.username || '';
        document.getElementById('mailPassword').value = settings.id ? '********' : '';
        document.getElementById('mailSecurityType').value = settings.securityType || 'TLS';
        document.getElementById('mailSystemActive').checked = settings.isActive || false;
    }

    async loadSmtpPresets() {
        try {
            console.log('Loading SMTP presets...');
            
            const response = await this.apiCall('/mailsettings/presets');

            console.log('Presets response status:', response.status);
            
            if (response.ok) {
                const presets = await response.json();
                console.log('Loaded presets:', presets);
                const presetsContainer = document.getElementById('smtpPresets');
                
                if (!presetsContainer) {
                    console.error('Presets container not found!');
                    return;
                }
                
                presetsContainer.innerHTML = presets.map((preset, index) => `
                    <li>
                        <a class="dropdown-item" href="#" data-preset-index="${index}">
                            <strong>${preset.name}</strong><br>
                            <small class="text-muted">${preset.description}</small>
                        </a>
                    </li>
                `).join('');
                
                console.log('Presets HTML updated, added', presets.length, 'presets');
                
                // Add event listeners for presets
                presetsContainer.addEventListener('click', (e) => {
                    e.preventDefault();
                    const link = e.target.closest('[data-preset-index]');
                    if (link) {
                        const index = parseInt(link.dataset.presetIndex);
                        const preset = presets[index];
                        this.applySmtpPreset(preset.name, preset.smtpServer, preset.port, preset.securityType);
                    }
                });
            } else {
                const errorText = await response.text();
                console.error('Failed to load presets:', response.status, errorText);
                if (response.status === 401) {
                    console.error('Unauthorized access to presets');
                }
            }
        } catch (error) {
            console.error('Error loading SMTP presets:', error);
        }
    }

    applySmtpPreset(name, server, port, security) {
        document.getElementById('mailSmtpServer').value = server;
        document.getElementById('mailPort').value = port;
        document.getElementById('mailSecurityType').value = security;
        
        // Close dropdown
        const dropdownBtn = document.querySelector('[data-bs-toggle="dropdown"]');
        if (dropdownBtn) {
            const dropdown = bootstrap.Dropdown.getInstance(dropdownBtn);
            if (dropdown) dropdown.hide();
        }
        
        this.showSuccess(`${name} ayarları uygulandı`);
    }

    async saveMailSettings(event) {
        event.preventDefault();
        
        const formData = {
            senderName: document.getElementById('mailSenderName').value,
            senderEmail: document.getElementById('mailSenderEmail').value,
            smtpServer: document.getElementById('mailSmtpServer').value,
            port: parseInt(document.getElementById('mailPort').value),
            username: document.getElementById('mailUsername').value,
            password: document.getElementById('mailPassword').value,
            securityType: document.getElementById('mailSecurityType').value,
            isActive: document.getElementById('mailSystemActive').checked
        };

        try {
            const response = await fetch(`${this.apiBase}/mailsettings`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.token}`
                },
                body: JSON.stringify(formData)
            });

            if (response.ok) {
                this.showSuccess('Mail ayarları başarıyla kaydedildi');
                this.loadMailSettings(); // Reload to get updated status
            } else {
                const error = await response.text();
                this.showError('Mail ayarları kaydedilemedi: ' + error);
            }
        } catch (error) {
            console.error('Error saving mail settings:', error);
            this.showError('Mail ayarları kaydedilirken hata oluştu');
        }
    }

    async testMailConnection() {
        const formData = {
            senderName: document.getElementById('mailSenderName').value,
            senderEmail: document.getElementById('mailSenderEmail').value,
            smtpServer: document.getElementById('mailSmtpServer').value,
            port: parseInt(document.getElementById('mailPort').value),
            username: document.getElementById('mailUsername').value,
            password: document.getElementById('mailPassword').value,
            securityType: document.getElementById('mailSecurityType').value,
            isActive: false
        };

        const btn = document.getElementById('testConnectionBtn');
        const originalText = btn.innerHTML;
        btn.innerHTML = '<i class="bi bi-hourglass-split"></i> Test ediliyor...';
        btn.disabled = true;

        try {
            const response = await fetch(`${this.apiBase}/mailsettings/test-connection`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.token}`
                },
                body: JSON.stringify(formData)
            });

            const result = await response.json();
            
            if (result.success) {
                this.showSuccess('Bağlantı başarılı! SMTP ayarları doğru.');
            } else {
                this.showError('Bağlantı başarısız: ' + result.message);
            }
        } catch (error) {
            console.error('Error testing mail connection:', error);
            this.showError('Bağlantı test edilirken hata oluştu');
        } finally {
            btn.innerHTML = originalText;
            btn.disabled = false;
        }
    }

    showTestMailModal() {
        const modal = new bootstrap.Modal(document.getElementById('testMailModal'));
        modal.show();
    }

    async sendTestMail() {
        const testMailData = {
            toEmail: document.getElementById('testMailTo').value,
            subject: document.getElementById('testMailSubject').value,
            message: document.getElementById('testMailMessage').value
        };

        const settingsData = {
            senderName: document.getElementById('mailSenderName').value,
            senderEmail: document.getElementById('mailSenderEmail').value,
            smtpServer: document.getElementById('mailSmtpServer').value,
            port: parseInt(document.getElementById('mailPort').value),
            username: document.getElementById('mailUsername').value,
            password: document.getElementById('mailPassword').value,
            securityType: document.getElementById('mailSecurityType').value,
            isActive: false
        };

        const btn = document.getElementById('sendTestMailConfirm');
        const originalText = btn.innerHTML;
        btn.innerHTML = '<i class="bi bi-hourglass-split"></i> Gönderiliyor...';
        btn.disabled = true;

        try {
            const response = await fetch(`${this.apiBase}/mailsettings/send-test`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${this.token}`
                },
                body: JSON.stringify({
                    settings: settingsData,
                    testMail: testMailData
                })
            });

            const result = await response.json();
            
            if (result.success) {
                this.showSuccess('Test maili başarıyla gönderildi!');
                const modal = bootstrap.Modal.getInstance(document.getElementById('testMailModal'));
                modal.hide();
            } else {
                this.showError('Test maili gönderilemedi: ' + result.message);
            }
        } catch (error) {
            console.error('Error sending test mail:', error);
            this.showError('Test maili gönderilirken hata oluştu');
        } finally {
            btn.innerHTML = originalText;
            btn.disabled = false;
        }
    }

    async deactivateMailSettings() {
        if (!confirm('Mail sistemini devre dışı bırakmak istediğinizden emin misiniz?')) {
            return;
        }

        try {
            const response = await fetch(`${this.apiBase}/mailsettings/deactivate`, {
                method: 'POST',
                headers: {
                    'Authorization': `Bearer ${this.token}`
                }
            });

            const result = await response.json();
            
            if (result.success) {
                this.showSuccess('Mail sistemi devre dışı bırakıldı');
                this.loadMailSettings();
            } else {
                this.showError('İşlem başarısız: ' + result.message);
            }
        } catch (error) {
            console.error('Error deactivating mail settings:', error);
            this.showError('İşlem sırasında hata oluştu');
        }
    }

    togglePasswordVisibility() {
        const passwordField = document.getElementById('mailPassword');
        const toggleBtn = document.getElementById('togglePassword');
        const icon = toggleBtn.querySelector('i');
        
        if (passwordField.type === 'password') {
            passwordField.type = 'text';
            icon.className = 'bi bi-eye-slash';
        } else {
            passwordField.type = 'password';
            icon.className = 'bi bi-eye';
        }
    }
}

// Initialize admin app when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    window.admin = new AdminApp();
});