using Sales.BUS;
using Sales.DAL;
using Sales.DTO;
using System.ComponentModel;
using System.Globalization;

namespace Sales.GUI;

public class MainForm : Form
{
    private readonly ProductService products = new();
    private readonly CustomerService customers = new();
    private readonly EmployeeService employees = new();
    private readonly SalesService sales = new();

    private readonly DataGridView gridProducts = MakeGrid();
    private readonly DataGridView gridCustomers = MakeGrid();
    private readonly DataGridView gridEmployees = MakeGrid();
    private readonly DataGridView gridCart = MakeGrid();
    private readonly TextBox txtSearchProduct = new() { Width = 240, PlaceholderText = "Nhập mã hoặc tên sản phẩm..." };
    private readonly ComboBox cboSaleProduct = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 330 };
    private readonly NumericUpDown nudQuantity = new() { Minimum = 1, Maximum = 100000, Value = 1, Width = 90 };
    private readonly ComboBox cboEmployee = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly ComboBox cboCustomer = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly BindingList<CartItem> cart = new();
    private readonly Label lblTotal = new() { AutoSize = true, Font = new Font("Segoe UI", 13, FontStyle.Bold), Text = "Tổng tiền: 0 VND" };

    public MainForm()
    {
        Text = "HỆ THỐNG QUẢN LÝ BÁN HÀNG - BÀI TẬP 6.1";
        Width = 1200; Height = 760; MinimumSize = new Size(1000, 650);
        StartPosition = FormStartPosition.CenterScreen;
        var tabs = new TabControl { Dock = DockStyle.Fill };
        tabs.TabPages.Add(BuildProductsTab());
        tabs.TabPages.Add(BuildCustomersTab());
        tabs.TabPages.Add(BuildEmployeesTab());
        tabs.TabPages.Add(BuildSalesTab());
        tabs.TabPages.Add(BuildReportsTab());
        Controls.Add(tabs);
        Shown += async (_, _) => await Safe(InitializeAsync);
    }

    private static DataGridView MakeGrid() => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false,
        AllowUserToDeleteRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect, MultiSelect = false,
        RowHeadersVisible = false
    };

    private static Button Btn(string text, EventHandler click, int width = 105)
    {
        var b = new Button { Text = text, AutoSize = false, Width = width, Height = 34, Margin = new Padding(4) };
        b.Click += click; return b;
    }

    private static FlowLayoutPanel Bar() => new()
    {
        Dock = DockStyle.Top, Height = 52, Padding = new Padding(6),
        FlowDirection = FlowDirection.LeftToRight, WrapContents = false
    };

    private TabPage BuildProductsTab()
    {
        var page = new TabPage("Sản phẩm"); var bar = Bar();
        bar.Controls.Add(txtSearchProduct);
        bar.Controls.Add(Btn("Tìm kiếm", async (_, _) => await Safe(LoadProductsAsync)));
        bar.Controls.Add(Btn("Tải lại", async (_, _) => { txtSearchProduct.Clear(); await Safe(LoadProductsAsync); }));
        bar.Controls.Add(Btn("Thêm", async (_, _) => await Safe(AddProductAsync)));
        bar.Controls.Add(Btn("Sửa", async (_, _) => await Safe(EditProductAsync)));
        bar.Controls.Add(Btn("Xóa", async (_, _) => await Safe(DeleteProductAsync)));
        page.Controls.Add(gridProducts); page.Controls.Add(bar); return page;
    }

    private TabPage BuildCustomersTab()
    {
        var page = new TabPage("Khách hàng"); var bar = Bar();
        bar.Controls.Add(Btn("Tải lại", async (_, _) => await Safe(LoadCustomersAsync)));
        bar.Controls.Add(Btn("Thêm", async (_, _) => await Safe(AddCustomerAsync)));
        bar.Controls.Add(Btn("Sửa", async (_, _) => await Safe(EditCustomerAsync)));
        bar.Controls.Add(Btn("Xóa", async (_, _) => await Safe(DeleteCustomerAsync)));
        page.Controls.Add(gridCustomers); page.Controls.Add(bar); return page;
    }

    private TabPage BuildEmployeesTab()
    {
        var page = new TabPage("Nhân viên"); var bar = Bar();
        bar.Controls.Add(Btn("Tải lại", async (_, _) => await Safe(LoadEmployeesAsync)));
        bar.Controls.Add(Btn("Thêm", async (_, _) => await Safe(AddEmployeeAsync)));
        bar.Controls.Add(Btn("Sửa", async (_, _) => await Safe(EditEmployeeAsync)));
        bar.Controls.Add(Btn("Xóa", async (_, _) => await Safe(DeleteEmployeeAsync)));
        page.Controls.Add(gridEmployees); page.Controls.Add(bar); return page;
    }

    private TabPage BuildSalesTab()
    {
        var page = new TabPage("Bán hàng");
        var top = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 112, Padding = new Padding(10), WrapContents = true };
        top.Controls.Add(new Label { Text = "Nhân viên:", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
        top.Controls.Add(cboEmployee);
        top.Controls.Add(new Label { Text = "Khách hàng (không bắt buộc):", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
        top.Controls.Add(cboCustomer);
        top.Controls.Add(new Label { Text = "Sản phẩm:", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
        top.Controls.Add(cboSaleProduct);
        top.Controls.Add(new Label { Text = "Số lượng:", AutoSize = true, Padding = new Padding(0, 9, 0, 0) });
        top.Controls.Add(nudQuantity);
        top.Controls.Add(Btn("Thêm vào giỏ", (_, _) => AddToCart()));
        gridCart.DataSource = cart;
        var bottom = new FlowLayoutPanel { Dock = DockStyle.Bottom, Height = 58, Padding = new Padding(10) };
        bottom.Controls.Add(lblTotal);
        bottom.Controls.Add(Btn("Xóa sản phẩm chọn", (_, _) => RemoveCartItem(), 150));
        bottom.Controls.Add(Btn("Làm mới giỏ", (_, _) => ClearCart()));
        bottom.Controls.Add(Btn("THANH TOÁN", async (_, _) => await Safe(CheckoutAsync), 140));
        page.Controls.Add(gridCart); page.Controls.Add(bottom); page.Controls.Add(top);
        return page;
    }

    private TabPage BuildReportsTab()
    {
        var page = new TabPage("Thống kê");
        var bar = new FlowLayoutPanel { Dock = DockStyle.Top, Height = 65, Padding = new Padding(10) };
        bar.Controls.Add(Btn("Doanh thu hôm nay", async (_, _) => await Safe(async () =>
        {
            var d = DateTime.Today; var value = await sales.RevenueAsync(d, d.AddDays(1));
            MessageBox.Show($"Doanh thu ngày {d:dd/MM/yyyy}: {value:N0} VND", "Doanh thu");
        }), 165));
        bar.Controls.Add(Btn("Doanh thu tháng này", async (_, _) => await Safe(async () =>
        {
            var d = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
            var value = await sales.RevenueAsync(d, d.AddMonths(1));
            MessageBox.Show($"Doanh thu tháng {d:MM/yyyy}: {value:N0} VND", "Doanh thu");
        }), 175));
        bar.Controls.Add(Btn("Top 5 sản phẩm", async (_, _) => await Safe(async () =>
        {
            var data = await sales.TopProductsAsync();
            using var f = new Form { Text = "Top 5 sản phẩm bán chạy", Width = 720, Height = 420, StartPosition = FormStartPosition.CenterParent };
            f.Controls.Add(new DataGridView { Dock = DockStyle.Fill, DataSource = data, ReadOnly = true, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill });
            f.ShowDialog(this);
        }), 150));
        page.Controls.Add(bar);
        page.Controls.Add(new Label { Dock = DockStyle.Top, Height = 55, Padding = new Padding(12), Text = "Báo cáo doanh thu sử dụng ExecuteScalar; Top 5 sản phẩm sử dụng SqlDataReader." });
        return page;
    }

    private async Task InitializeAsync()
    {
        await Db.TestConnectionAsync();
        await LoadProductsAsync(); await LoadCustomersAsync(); await LoadEmployeesAsync();
        Text += " - Đã kết nối SalesDB";
    }

    private async Task LoadProductsAsync()
    {
        gridProducts.DataSource = await products.GetAllAsync(txtSearchProduct.Text.Trim());
        var list = await products.GetAllAsync();
        cboSaleProduct.DataSource = null; cboSaleProduct.DataSource = list;
        cboSaleProduct.DisplayMember = nameof(Product.TenSP); cboSaleProduct.ValueMember = nameof(Product.MaSP);
    }
    private async Task LoadCustomersAsync()
    {
        gridCustomers.DataSource = await customers.GetAllAsync();
        var list = await customers.GetAllAsync();
        cboCustomer.DataSource = null; cboCustomer.Items.Clear();
        cboCustomer.Items.Add(new Customer { MaKH = 0, TenKH = "-- Khách lẻ --" });
        foreach (var c in list) cboCustomer.Items.Add(c);
        cboCustomer.DisplayMember = nameof(Customer.TenKH); cboCustomer.SelectedIndex = 0;
    }
    private async Task LoadEmployeesAsync()
    {
        gridEmployees.DataSource = await employees.GetAllAsync();
        var list = await employees.GetAllAsync();
        cboEmployee.DataSource = list; cboEmployee.DisplayMember = nameof(Employee.TenNV); cboEmployee.ValueMember = nameof(Employee.MaNV);
    }

    private async Task AddProductAsync()
    {
        var v = EditDialog.Show(this, "Thêm sản phẩm", ("Mã sản phẩm", ""), ("Tên sản phẩm", ""), ("Giá bán", ""), ("Số lượng tồn", ""));
        if (v == null) return;
        if (!decimal.TryParse(v[2], NumberStyles.Number, CultureInfo.CurrentCulture, out var price) ||
            !int.TryParse(v[3], out var stock)) throw new ArgumentException("Giá bán hoặc số lượng không hợp lệ.");
        await products.SaveAsync(new Product { MaSP = v[0], TenSP = v[1], GiaBan = price, SoLuongTon = stock }, true);
        await LoadProductsAsync();
    }
    private async Task EditProductAsync()
    {
        if (gridProducts.CurrentRow?.DataBoundItem is not Product p) throw new InvalidOperationException("Hãy chọn sản phẩm cần sửa.");
        var v = EditDialog.Show(this, "Sửa sản phẩm", ("Mã sản phẩm", p.MaSP, true), ("Tên sản phẩm", p.TenSP), ("Giá bán", p.GiaBan.ToString(CultureInfo.CurrentCulture)), ("Số lượng tồn", p.SoLuongTon.ToString()));
        if (v == null) return;
        if (!decimal.TryParse(v[2], NumberStyles.Number, CultureInfo.CurrentCulture, out var price) || !int.TryParse(v[3], out var stock)) throw new ArgumentException("Giá bán hoặc số lượng không hợp lệ.");
        await products.SaveAsync(new Product { MaSP = p.MaSP, TenSP = v[1], GiaBan = price, SoLuongTon = stock }, false);
        await LoadProductsAsync();
    }
    private async Task DeleteProductAsync()
    {
        if (gridProducts.CurrentRow?.DataBoundItem is not Product p) throw new InvalidOperationException("Hãy chọn sản phẩm cần xóa.");
        if (MessageBox.Show($"Xóa sản phẩm {p.TenSP}?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        await products.DeleteAsync(p.MaSP); await LoadProductsAsync();
    }

    private async Task AddCustomerAsync()
    {
        var v = EditDialog.Show(this, "Thêm khách hàng", ("Tên khách hàng", ""), ("Điện thoại", ""));
        if (v == null) return;
        await customers.SaveAsync(new Customer { TenKH = v[0], DienThoai = NullIfEmpty(v[1]) }, true);
        await LoadCustomersAsync();
    }
    private async Task EditCustomerAsync()
    {
        if (gridCustomers.CurrentRow?.DataBoundItem is not Customer c) throw new InvalidOperationException("Hãy chọn khách hàng cần sửa.");
        var v = EditDialog.Show(this, "Sửa khách hàng", ("Tên khách hàng", c.TenKH), ("Điện thoại", c.DienThoai ?? ""));
        if (v == null) return;
        await customers.SaveAsync(new Customer { MaKH = c.MaKH, TenKH = v[0], DienThoai = NullIfEmpty(v[1]) }, false);
        await LoadCustomersAsync();
    }
    private async Task DeleteCustomerAsync()
    {
        if (gridCustomers.CurrentRow?.DataBoundItem is not Customer c) throw new InvalidOperationException("Hãy chọn khách hàng cần xóa.");
        if (MessageBox.Show($"Xóa khách hàng {c.TenKH}?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        await customers.DeleteAsync(c.MaKH); await LoadCustomersAsync();
    }

    private async Task AddEmployeeAsync()
    {
        var v = EditDialog.Show(this, "Thêm nhân viên", ("Tên nhân viên", ""), ("Số điện thoại", ""), ("Chức vụ", ""));
        if (v == null) return;
        await employees.SaveAsync(new Employee { TenNV = v[0], SoDienThoai = NullIfEmpty(v[1]), ChucVu = NullIfEmpty(v[2]) }, true);
        await LoadEmployeesAsync();
    }
    private async Task EditEmployeeAsync()
    {
        if (gridEmployees.CurrentRow?.DataBoundItem is not Employee e) throw new InvalidOperationException("Hãy chọn nhân viên cần sửa.");
        var v = EditDialog.Show(this, "Sửa nhân viên", ("Tên nhân viên", e.TenNV), ("Số điện thoại", e.SoDienThoai ?? ""), ("Chức vụ", e.ChucVu ?? ""));
        if (v == null) return;
        await employees.SaveAsync(new Employee { MaNV = e.MaNV, TenNV = v[0], SoDienThoai = NullIfEmpty(v[1]), ChucVu = NullIfEmpty(v[2]) }, false);
        await LoadEmployeesAsync();
    }
    private async Task DeleteEmployeeAsync()
    {
        if (gridEmployees.CurrentRow?.DataBoundItem is not Employee e) throw new InvalidOperationException("Hãy chọn nhân viên cần xóa.");
        if (MessageBox.Show($"Xóa nhân viên {e.TenNV}?", "Xác nhận", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
        await employees.DeleteAsync(e.MaNV); await LoadEmployeesAsync();
    }

    private void AddToCart()
    {
        if (cboSaleProduct.SelectedItem is not Product p) throw new InvalidOperationException("Chưa có sản phẩm. Hãy thêm sản phẩm trước.");
        int qty = (int)nudQuantity.Value;
        if (qty > p.SoLuongTon) throw new InvalidOperationException($"Tồn kho của {p.TenSP} chỉ còn {p.SoLuongTon}.");
        var existing = cart.FirstOrDefault(x => x.MaSP == p.MaSP);
        if (existing != null)
        {
            if (existing.SoLuong + qty > p.SoLuongTon) throw new InvalidOperationException("Số lượng trong giỏ vượt quá tồn kho.");
            existing.SoLuong += qty;
        }
        else cart.Add(new CartItem { MaSP = p.MaSP, TenSP = p.TenSP, DonGia = p.GiaBan, SoLuong = qty });
        gridCart.Refresh(); UpdateTotal();
    }
    private void RemoveCartItem()
    {
        if (gridCart.CurrentRow?.DataBoundItem is CartItem item) cart.Remove(item);
        UpdateTotal();
    }
    private void ClearCart() { cart.Clear(); UpdateTotal(); }
    private void UpdateTotal() => lblTotal.Text = $"Tổng tiền: {cart.Sum(x => x.ThanhTien):N0} VND";

    private async Task CheckoutAsync()
    {
        if (cboEmployee.SelectedItem is not Employee employee) throw new InvalidOperationException("Cần có ít nhất một nhân viên trước khi thanh toán.");
        if (cart.Count == 0) throw new InvalidOperationException("Giỏ hàng đang trống.");
        int? customerId = cboCustomer.SelectedItem is Customer customer && customer.MaKH > 0 ? customer.MaKH : null;
        var total = cart.Sum(x => x.ThanhTien);
        if (MessageBox.Show($"Xác nhận thanh toán {total:N0} VND?", "Thanh toán", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
        int invoiceId = await sales.CheckoutAsync(employee.MaNV, customerId, cart.ToList());
        MessageBox.Show($"Thanh toán thành công!\nMã hóa đơn: {invoiceId}\nTổng tiền: {total:N0} VND", "Hoàn tất", MessageBoxButtons.OK, MessageBoxIcon.Information);
        ClearCart(); await LoadProductsAsync(); await LoadCustomersAsync();
    }

    private static string? NullIfEmpty(string s) => string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    private async Task Safe(Func<Task> action)
    {
        try { await action(); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning); }
    }
}

internal sealed class EditDialog : Form
{
    private readonly List<TextBox> inputs = new();
    private EditDialog(string title, (string label, string value, bool readOnly)[] fields)
    {
        Text = title; Width = 470; Height = 150 + fields.Length * 58; StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(12), ColumnCount = 2, RowCount = fields.Length + 1 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 135)); layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int i = 0; i < fields.Length; i++)
        {
            var field = fields[i];
            layout.Controls.Add(new Label { Text = field.label, AutoSize = true, Anchor = AnchorStyles.Left }, 0, i);
            var input = new TextBox { Text = field.value, Dock = DockStyle.Fill, ReadOnly = field.readOnly, Margin = new Padding(3, 6, 3, 6) };
            inputs.Add(input); layout.Controls.Add(input, 1, i); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        }
        var buttons = new FlowLayoutPanel { FlowDirection = FlowDirection.RightToLeft, Dock = DockStyle.Fill };
        var ok = new Button { Text = "Lưu", DialogResult = DialogResult.OK, Width = 90 };
        var cancel = new Button { Text = "Hủy", DialogResult = DialogResult.Cancel, Width = 90 };
        buttons.Controls.Add(ok); buttons.Controls.Add(cancel); layout.Controls.Add(buttons, 0, fields.Length); layout.SetColumnSpan(buttons, 2);
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); Controls.Add(layout); AcceptButton = ok; CancelButton = cancel;
    }
    public static string[]? Show(IWin32Window owner, string title, params (string label, string value)[] fields)
    {
        var expanded = fields.Select(f => (f.label, f.value, false)).ToArray();
        using var dialog = new EditDialog(title, expanded);
        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.inputs.Select(x => x.Text.Trim()).ToArray() : null;
    }
    public static string[]? Show(IWin32Window owner, string title, (string label, string value, bool readOnly) first, params (string label, string value)[] rest)
    {
        var all = new List<(string label, string value, bool readOnly)> { first };
        all.AddRange(rest.Select(x => (x.label, x.value, false)));
        using var dialog = new EditDialog(title, all.ToArray());
        return dialog.ShowDialog(owner) == DialogResult.OK ? dialog.inputs.Select(x => x.Text.Trim()).ToArray() : null;
    }
}
