class LoginApp {
    constructor() {
        this.apiBase = '/api';
        this.init();
    }

    init() {
        // Clear any existing auth data when login page loads
        this.clearAuthData();
        this.bindEvents();
    }

    bindEvents() {
        const loginForm = document.getElementById('loginForm');
        loginForm.addEventListener('submit', (e) => {
            e.preventDefault();
            this.login();
        });

        // Handle Enter key
        document.addEventListener('keypress', (e) => {
            if (e.key === 'Enter') {
                e.preventDefault();
                this.login();
            }
        });
    }

    async login() {
        const username = document.getElementById('username').value.trim();
        const password = document.getElementById('password').value;

        if (!username || !password) {
            this.showError('Kullanıcı adı ve şifre gereklidir');
            return;
        }

        try {
            const response = await fetch(`${this.apiBase}/auth/login`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({ username, password })
            });

            const result = await response.json();

            if (response.ok) {
                // Store authentication data
                localStorage.setItem('authToken', result.token);
                localStorage.setItem('user', JSON.stringify(result.user));
                
                // Success message
                this.showSuccess('Giriş başarılı, yönlendiriliyor...');
                
                // Redirect after short delay
                setTimeout(() => {
                    this.redirectUser(result.user);
                }, 1000);
            } else {
                this.showError(result.message || 'Giriş başarısız');
            }
        } catch (error) {
            console.error('Login error:', error);
            this.showError(`Bağlantı hatası: ${error.message}. API URL: ${this.apiBase}/auth/login`);
        }
    }

    redirectUser(user) {
        if (user.role === 'Admin' || user.role === 'Manager') {
            window.location.href = 'admin.html';
        } else {
            window.location.href = 'visitor.html';
        }
    }

    clearAuthData() {
        localStorage.removeItem('authToken');
        localStorage.removeItem('user');
    }

    showError(message) {
        const errorAlert = document.getElementById('errorAlert');
        const errorMessage = document.getElementById('errorMessage');
        
        errorMessage.textContent = message;
        errorAlert.style.display = 'block';
        
        setTimeout(() => {
            errorAlert.style.display = 'none';
        }, 5000);
    }

    showSuccess(message) {
        const errorAlert = document.getElementById('errorAlert');
        errorAlert.className = 'alert alert-success';
        errorAlert.querySelector('i').className = 'bi bi-check-circle me-2';
        
        const errorMessage = document.getElementById('errorMessage');
        errorMessage.textContent = message;
        errorAlert.style.display = 'block';
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new LoginApp();
});