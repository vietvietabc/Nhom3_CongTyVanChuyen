const apiBaseUrl = "https://localhost:7113/api";    

let nhanVienDangChon = new Set();

function loadQuanLyNhanVien() {
    const mainContent = document.getElementById('mainContent');
    mainContent.innerHTML = `
<div class="d-flex justify-content-between align-items-center mb-4">
    <h2>Danh sách nhân viên</h2>
    <div>
        <button class="btn btn-danger me-2" onclick="xoaNhieuNhanVien()">Xóa nhân viên đã chọn</button>
        <button class="btn btn-primary" onclick="taoTaiKhoan()">+ Tạo nhân viên</button>
    </div>
</div>
<div class="table-responsive">
    <table class="table table-bordered table-hover align-middle">
        <thead class="table-light">
            <tr>
                <th><input type="checkbox" id="selectAll" onchange="chonTatCa(this)"></th>
                <th>STT</th>
                <th>Mã nhân viên</th>
                <th>Họ tên</th>
                <th>Email</th>
                <th>Điện thoại</th>
                <th>Chức vụ</th>
                <th>Địa chỉ</th>
                <th>Thao tác</th>
            </tr>
        </thead>
        <tbody id="nhanVienTableBody">
        </tbody>
    </table>
</div>
    `;

    loadDanhSachNhanVien();
}

async function loadDanhSachNhanVien() {
    const tbody = document.getElementById('nhanVienTableBody');
    tbody.innerHTML = `<tr><td colspan="9" class="text-center">Đang tải...</td></tr>`;

    try {
        const response = await fetch(`${apiBaseUrl}/NhanVien`);
        const data = await response.json();
        console.log(data); // Kiểm tra dữ liệu trả về từ API

        tbody.innerHTML = '';

        if (!data || data.length === 0) {
            tbody.innerHTML = `<tr><td colspan="9" class="text-center">Không có nhân viên nào.</td></tr>`;
            return;
        }

        data.forEach((nv, index) => {
            const tr = document.createElement('tr');
            tr.innerHTML = `
                <td><input type="checkbox" class="chonNhanVien" value="${nv.id || ''}" onchange="chonNhanVien(this)"></td>
                <td>${index + 1}</td>
                <td>${nv.maNhanVien || 'Không có mã'}</td>
                <td>${nv.tenNhanVien || 'Không có tên'}</td>
                <td>${nv.email || 'Không có email'}</td>
                <td>${nv.sdt || 'Không có số điện thoại'}</td>
                <td>${nv.tenVaiTro || 'Không có chức vụ'}</td>
                <td>${nv.diaChiSoNha || 'Không có địa chỉ'}</td>
                <td>
                    <button class="btn btn-sm btn-warning" onclick="suaNhanVien(${nv.id})">Sửa</button>
                    <button class="btn btn-sm btn-danger" onclick="xoaNhanVien(${nv.id})">Xóa</button>
                </td>
            `;
            tbody.appendChild(tr);
        });
    } catch (error) {
        console.error('Lỗi tải nhân viên:', error);
        tbody.innerHTML = `<tr><td colspan="9" class="text-center">Không thể tải dữ liệu.</td></tr>`;
    }
}

function chonTatCa(checkbox) {
    const allCheckboxes = document.querySelectorAll('.chonNhanVien');
    nhanVienDangChon.clear();
    allCheckboxes.forEach(cb => {
        cb.checked = checkbox.checked;
        if (checkbox.checked) {
            nhanVienDangChon.add(cb.value);
        }
    });
}

function chonNhanVien(checkbox) {
    if (checkbox.checked) {
        if (checkbox.value) { // Chỉ thêm nếu value không phải undefined hoặc rỗng
            nhanVienDangChon.add(checkbox.value);
        }
    } else {
        nhanVienDangChon.delete(checkbox.value);
    }

    const allCheckboxes = document.querySelectorAll('.chonNhanVien');
    const allChecked = Array.from(allCheckboxes).every(cb => cb.checked);
    document.getElementById('selectAll').checked = allChecked;
}

async function xoaNhieuNhanVien() {
    if (nhanVienDangChon.size === 0) {
        alert('Vui lòng chọn ít nhất 1 nhân viên để xóa.');
        return;
    }

    if (!confirm('Bạn chắc chắn muốn xóa những nhân viên đã chọn?')) return;

    const errors = [];
    try {
        for (let id of nhanVienDangChon) {
            if (!id) continue; // Bỏ qua các giá trị không hợp lệ
            const response = await fetch(`${apiBaseUrl}/NhanVien/${id}`, { method: 'DELETE' });
            if (!response.ok) {
                errors.push(id);
            }
        }

        if (errors.length > 0) {
            alert(`Không thể xóa các nhân viên với ID: ${errors.join(', ')}.`);
        } else {
            alert('Xóa thành công!');
        }
        loadQuanLyNhanVien();
    } catch (error) {
        console.error('Lỗi xóa:', error);
        alert('Xóa thất bại! Vui lòng thử lại sau.');
    }
}

async function xoaNhanVien(id) {
    if (!confirm('Bạn chắc chắn muốn xóa nhân viên này?')) return;

    try {
        const response = await fetch(`${apiBaseUrl}/NhanVien/${id}`, { method: 'DELETE' });
        if (response.ok) {
            alert('Xóa thành công!');
            loadQuanLyNhanVien();
        } else {
            alert('Xóa thất bại!');
        }
    } catch (error) {
        console.error('Lỗi xóa:', error);
        alert('Xóa thất bại! Vui lòng thử lại sau.');
    }
}
    
function suaNhanVien(id) {
    // Điều hướng sang trang sửa nhân viên với ID cụ thể
    window.location.href = `/Admin/suanhanvien.html?id=${id}`;
}

function taoTaiKhoan() {
    // Điều hướng sang trang tạo tài khoản
    window.location.href = "/Admin/taotaikhoan.html";
}
