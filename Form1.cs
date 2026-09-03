    using ReaLTaiizor.Extension;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics.Eventing.Reader;
    using System.Drawing;
    using System.Drawing.Design;
    using System.Drawing.Drawing2D;
    using System.Drawing.Printing;
    using System.Drawing.Text;
    using System.Net;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Windows.Forms;


namespace POS
{
    public partial class PosSystem : Form
    {
        private DataGridViewRow? reportRowToPrint;

        private bool _layoutReady = false;

        const int expandedHeight = 180;
        const int constHeight = 40;

        private Panel? slidingPanel = null;
        private int targetHeight = 40;

        private const int PanelClosedHeight = 40;
        private const int PanelOpenHeight = 345;
        private const int PanelSlideSpeed = 20;

        bool tabMenuExpanded = false;

        ConnectionDB _db = new ConnectionDB();

        ToolTip pop = new ToolTip();
        public PosSystem()
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.KeyPreview = true;

            InitializeComponent();

            this.MinimumSize = new Size(1024, 768);
            this.ClientSize = new Size(1443, 818);

            changePass_txt.Enabled = false;
            updatePass_btn.Enabled = false;
            endTransac_btn.Enabled = false;

            print_btn.Enabled = false;

            otpCode_txt.TextChanged += otpCode_txt_TextChanged;

            inventory_dataGrid.DefaultValuesNeeded += inventory_dataGrid_DefaultValuesNeeded;

            this.Paint += new System.Windows.Forms.PaintEventHandler(BackGroundColor.Display_BGColor);
            this.MaximizeBox = true;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            settingTimer.Interval = 15;

            Location_Panel();

            EnableDoubleBuffer(this);
            EnableDoubleBuffer(TabMenu);
            EnableDoubleBuffer(Menu_panel1);

            EnableDoubleBuffer(dash_panel);
            EnableDoubleBuffer(sales_panelboard);
            EnableDoubleBuffer(invent_panel);

            EnableDoubleBuffer(Dashboard_panel);
            EnableDoubleBuffer(Sales_panel);
            EnableDoubleBuffer(Inventory_panel);

            Menu_panel1.Visible = false;
            TabMenu.Visible = false;
            setup_panel.Visible = true;
            login_panel.Visible = false;
            dash_panel.Visible = false;
            invent_panel.Visible = false;
            sales_panelboard.Visible = false;
            setting_panel.Visible = false;
            report_panel.Visible = false;
            help_richTextBox.Visible = false;

        }

        private void PosSystem_Load(object sender, EventArgs e)
        {
            AppName.CreateTable();
            Setup.CreateTable();
            Inventory.CreateTable();
            Sales.CreateTable();
            Reports.GenerateReport();

            LoadReports();
            Color_IconUI();
            Highlight_MouseHover();
            Label_MouseHover();
            Location_Panel();
            LoadData();
            LoadSaleProducts();
            SetupSaleSummary();
            LoadTodaysSales();
            EnableAddAccount();
            LoadHelpGuide();

            _db.GetConnection();

            print_Toggle.Checked = AppName.GetPrintEnabled();

            version_lbl.Text = $"iTinda.myPOS - Powered by rei.MIND - Version {Application.ProductVersion}";

            print_lbl.Text =
                print_Toggle.Checked
                    ? "Printing Enabled"
                    : "Printing Disabled";

            TabMenu.Height = constHeight;

            sale_dataGrid.DataSource = Sales.GetProducts();

            password_txt.UseSystemPasswordChar = true;
            UserOperator_lbl.Visible = false;
            indicator_online.Visible = false;
            otpExpire_lbl.Visible = false;

            endTransac_btn.ForeColor = Color.Red;

            cash_txt.Enabled = false;

            pop.AutomaticDelay = 3000;
            pop.InitialDelay = 1000;
            pop.ReshowDelay = 1000;
            pop.OwnerDraw = true;
            pop.Draw += Pop_upDraw;
            pop.Popup += Pop_upTooltipSize;

            Dashboard_panel.Paint += Curve;
            Sales_panel.Paint += Curve;
            Inventory_panel.Paint += Curve;

            _layoutReady = true;

            ResizeEntireUI();

            this.BeginInvoke(new Action(() =>
            {
                ResizeEntireUI();
            }));

            ChangeApp_panel.Height = PanelClosedHeight;
            changePass_panel.Height = PanelClosedHeight;
            addAccount_panel.Height = PanelClosedHeight;
            reset_panel.Height = PanelClosedHeight;

            string appName = AppName.GetAppName();

            this.Text = appName;
            brandName_lbl1.Text = appName;
            brandName_lbl2.Text = appName;
        }

        private void EnableDoubleBuffer(Control control)
        {
            typeof(Control)
                .GetProperty(
                    "DoubleBuffered",
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.NonPublic)
                ?.SetValue(control, true, null);
        }

        public void Curve(object? sender, PaintEventArgs e)
        {
            Panel? panel = (Panel?)sender;
            if (panel != null)
            {
                int radius = 30;

                GraphicsPath path = new GraphicsPath();
                path.AddArc(0, 0, radius, radius, 180, 90);
                path.AddArc(panel.Width - radius, 0, radius, radius, 270, 90);
                path.AddArc(panel.Width - radius, panel.Height - radius, radius, radius, 0, 90);
                path.AddArc(0, panel.Height - radius, radius, radius, 90, 90);
                path.CloseFigure();

                panel.Region = new Region(path);
                using (Pen p = new Pen(Color.Gray, 10))
                {
                    e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                    e.Graphics.DrawPath(p, path);
                }
            }

        }

        private void Menu_panel1_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, Menu_panel1.ClientRectangle.Width, Menu_panel1.ClientRectangle.Height);
            using (SolidBrush brush = new SolidBrush(Color.Silver))
            {
                e.Graphics.FillRectangle(brush, rect);
            }

            using (Pen pen = new Pen(Color.Gray, 5))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawRectangle(pen, 0, 0, Menu_panel1.Width, Menu_panel1.Height);
            }

        }

        private void TabMenu_Click(object sender, EventArgs e)
        {
            tabMenuExpanded = !tabMenuExpanded;

            UpdateUserOperatorLabel();

            TimerEffect1.Interval = 15;
            TimerEffect1.Start();
        }

        private void Pop_upDraw(object? sender, DrawToolTipEventArgs e)
        {
            e.Graphics.FillRectangle(new SolidBrush(Color.Silver), e.Bounds);

            e.Graphics.DrawRectangle(new Pen(Color.Gray, 2), e.Bounds);

            e.Graphics.DrawString(e.ToolTipText, new Font("Segoe UI", 10, FontStyle.Italic), Brushes.Black, new Point(2, 3));
        }

        private void Pop_upTooltipSize(object? sender, PopupEventArgs p)
        {
            p.ToolTipSize = new Size(380, 30);
        }
        private void Highlight_MouseHover()
        {
            Dashboard_panel.MouseEnter += (s, e) =>
            {
                Dashboard_panel.BackColor = Color.Silver;
                dash_lbl.ForeColor = Color.Black;
                dash_Icon.ForeColor = Color.Black;

                pop.SetToolTip(Dashboard_panel, "View Today's sales, charts, and low stocks...");
            };
            Dashboard_panel.MouseLeave += (s, e) =>
            {
                Dashboard_panel.BackColor = Color.DarkSlateGray;
                dash_lbl.ForeColor = Color.DarkGoldenrod;
                dash_Icon.ForeColor = Color.DarkGoldenrod;
            };

            Sales_panel.MouseEnter += (s, e) =>
            {
                Sales_panel.BackColor = Color.Silver;
                sales_lbl.ForeColor = Color.Black;
                sales_Icon.ForeColor = Color.Black;

                pop.SetToolTip(Sales_panel, "Process orders and print receipts...");
            };
            Sales_panel.MouseLeave += (s, e) =>
            {
                Sales_panel.BackColor = Color.DarkSlateGray;
                sales_lbl.ForeColor = Color.DarkGoldenrod;
                sales_Icon.ForeColor = Color.DarkGoldenrod;
            };
            Inventory_panel.MouseEnter += (s, e) =>
            {
                Inventory_panel.BackColor = Color.Silver;
                inventory_lbl.ForeColor = Color.Black;
                inventory_Icon.ForeColor = Color.Black;

                pop.SetToolTip(Inventory_panel, "Manage products, Add stocks and check supplies...");

            };
            Inventory_panel.MouseLeave += (s, e) =>
            {
                Inventory_panel.BackColor = Color.DarkSlateGray;
                inventory_lbl.ForeColor = Color.DarkGoldenrod;
                inventory_Icon.ForeColor = Color.DarkGoldenrod;
            };
        }
        private void Label_MouseHover()
        {
            dash_lbl.MouseEnter += (s, e) =>
            {
                Dashboard_panel.BackColor = Color.Silver;
                dash_Icon.ForeColor = Color.Black;
                dash_lbl.ForeColor = Color.Black;

                pop.SetToolTip(Dashboard_panel, "View Today's sales, charts, and low stocks...");

            };
            dash_lbl.MouseLeave += (s, e) =>
            {
                Dashboard_panel.BackColor = Color.DarkSlateGray;
                dash_lbl.ForeColor = Color.DarkGoldenrod;
                dash_Icon.ForeColor = Color.DarkGoldenrod;

            };
            dash_Icon.MouseEnter += (s, e) =>
            {
                Dashboard_panel.BackColor = Color.Silver;
                dash_Icon.ForeColor = Color.Black;
                dash_lbl.ForeColor = Color.Black;

                pop.SetToolTip(Dashboard_panel, "View Today's sales, charts, and low stocks...");
            };
            dash_Icon.MouseLeave += (s, e) =>
            {
                Dashboard_panel.BackColor = Color.DarkSlateGray;
                dash_lbl.ForeColor = Color.DarkGoldenrod;
                dash_Icon.ForeColor = Color.DarkGoldenrod;
            };


            sales_lbl.MouseEnter += (s, e) =>
            {
                Sales_panel.BackColor = Color.Silver;
                sales_lbl.ForeColor = Color.Black;
                sales_Icon.ForeColor = Color.Black;

                pop.SetToolTip(Sales_panel, "Process orders and print receipts...");
            };
            sales_lbl.MouseLeave += (s, e) =>
            {
                Sales_panel.BackColor = Color.DarkSlateGray;
                sales_lbl.ForeColor = Color.DarkGoldenrod;
                sales_Icon.ForeColor = Color.DarkGoldenrod;
            };
            sales_Icon.MouseEnter += (s, e) =>
            {
                Sales_panel.BackColor = Color.Silver;
                sales_lbl.ForeColor = Color.Black;
                sales_Icon.ForeColor = Color.Black;

                pop.SetToolTip(Sales_panel, "Process orders and print receipts");
            };
            sales_Icon.MouseLeave += (s, e) =>
            {
                Sales_panel.BackColor = Color.DarkSlateGray;
                sales_lbl.ForeColor = Color.DarkGoldenrod;
                sales_Icon.ForeColor = Color.DarkGoldenrod;
            };


            inventory_lbl.MouseEnter += (s, e) =>
            {
                Inventory_panel.BackColor = Color.Silver;
                inventory_lbl.ForeColor = Color.Black;
                inventory_Icon.ForeColor = Color.Black;

                pop.SetToolTip(Inventory_panel, "Manage products, Add stocks and check supplies...");
            };
            inventory_lbl.MouseLeave += (s, e) =>
            {
                Inventory_panel.BackColor = Color.DarkSlateGray;
                inventory_lbl.ForeColor = Color.DarkGoldenrod;
                inventory_Icon.ForeColor = Color.DarkGoldenrod;
            };
            inventory_Icon.MouseEnter += (s, e) =>
            {
                Inventory_panel.BackColor = Color.Silver;
                inventory_lbl.ForeColor = Color.Black;
                inventory_Icon.ForeColor = Color.Black;

                pop.SetToolTip(Inventory_panel, "Manage products, Add stocks and check supplies...");
            };
            inventory_Icon.MouseLeave += (s, e) =>
            {
                Inventory_panel.BackColor = Color.DarkSlateGray;
                inventory_lbl.ForeColor = Color.DarkGoldenrod;
                inventory_Icon.ForeColor = Color.DarkGoldenrod;
            };

            eye_btn.MouseEnter += (s, e) =>
            {
                eye_btn.ForeColor = Color.Silver;
                password_txt.UseSystemPasswordChar = false;
            };
            eye_btn.MouseLeave += (s, e) =>
            {
                eye_btn.ForeColor = Color.Tan;
                password_txt.UseSystemPasswordChar = true;
            };
        }

        private void Color_IconUI()
        {
            dash_Icon.Text = "\uD83D\uDCCA";
            sales_Icon.Text = "\uD83D\uDCB0";
            inventory_Icon.Text = "\uD83D\uDCE6";
            eye_btn.Text = "\U0001F441";

            critical_lbl.Text = "Critical \U0001F534";
            critical_lbl.ForeColor = Color.Red;
            mod_lbl.Text = "Moderate \U0001f7e0";
            mod_lbl.ForeColor = Color.Orange;
            good_lbl.Text = "Good \U0001f7e2";
            good_lbl.ForeColor = Color.Green;

            Dashboard_panel.BackColor = Color.DarkSlateGray;
            Sales_panel.BackColor = Color.DarkSlateGray;
            Inventory_panel.BackColor = Color.DarkSlateGray;
            dash_lbl.ForeColor = Color.DarkGoldenrod;
            dash_Icon.ForeColor = Color.DarkGoldenrod;
            sales_lbl.ForeColor = Color.DarkGoldenrod;
            sales_Icon.ForeColor = Color.DarkGoldenrod;
            inventory_lbl.ForeColor = Color.DarkGoldenrod;
            inventory_Icon.ForeColor = Color.DarkGoldenrod;
        }

        private void TabMenu_Paint(object sender, PaintEventArgs e)
        {
            Rectangle rect = new Rectangle(0, 0, TabMenu.ClientRectangle.Width, TabMenu.ClientRectangle.Height);
            using (SolidBrush brush = new SolidBrush(Color.Silver))
            {
                e.Graphics.FillRectangle(brush, rect);
            }
        }
        private void TimerEffect1_Tick(object sender, EventArgs e)
        {
            int targetHeight = tabMenuExpanded
                ? expandedHeight
                : constHeight;

            int difference = targetHeight - TabMenu.Height;

            int step = Math.Max(1, Math.Abs(difference) / 5);

            if (difference > 0)
            {
                TabMenu.Height += step;
            }
            else if (difference < 0)
            {
                TabMenu.Height -= step;
            }

            if (Math.Abs(difference) <= 2)
            {
                TabMenu.Height = targetHeight;
                TimerEffect1.Stop();
            }
        }
        private void CenterPanel(Control panel)
        {
            if (panel == null)
                return;

            int availableHeight =
                this.ClientSize.Height - TabMenu.Height;

            panel.Left =
                (this.ClientSize.Width - panel.Width) / 2;

            panel.Top =
                TabMenu.Height +
                (availableHeight - panel.Height) / 2;
        }

        private void Location_Panel()
        {
            Menu_panel1.Size = new Size(925, 344);

            login_panel.Size = new Size(758, 491);

            setup_panel.Size = new Size(758, 491);

            dash_panel.Size = new Size(1246, 624);

            invent_panel.Size = new Size(1328, 557);

            sales_panelboard.Size = new Size(1315, 668);

            setting_panel.Size = new Size(1066, 578);

            report_panel.Size = new Size(1333, 634);

            help_richTextBox.Size = new Size(1350, 714);


            CenterPanel(Menu_panel1);
            CenterPanel(login_panel);
            CenterPanel(setup_panel);
            CenterPanel(dash_panel);
            CenterPanel(invent_panel);
            CenterPanel(sales_panelboard);
            CenterPanel(setting_panel);
            CenterPanel(report_panel);
            CenterPanel(help_richTextBox);

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }
        private void home_lbl_Click(object sender, EventArgs e)
        {
            Menu_panel1.Visible = true;
            login_panel.Visible = false;
            dash_panel.Visible = false;
            invent_panel.Visible = false;
            sales_panelboard.Visible = false;
            setting_panel.Visible = false;
            report_panel.Visible = false;
            help_richTextBox.Visible = false;


            home_lbl.ForeColor = Color.DarkGoldenrod;
            setting_lbl.ForeColor = Color.DarkSlateGray;
            reports_lbl.ForeColor = Color.DarkSlateGray;
            help_lbl.ForeColor = Color.DarkSlateGray;
        }
        private void login_btn_Click(object sender, EventArgs e)
        {
            string username = username_txt.Text;
            string password = password_txt.Text;

            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("");
            }

            bool isAuthenticated = Setup.Login(username, password);
            if (isAuthenticated)
            {
                Reports.SaveTimeIn(username);

                MessageBox.Show("Log in Success!.");
                login_panel.Visible = false;
                Menu_panel1.Visible = true;
                TabMenu.Enabled = true;
                UserOperator_lbl.Visible = true;
                indicator_online.Visible = true;
                TabMenu.Visible = true;
                LoginSuccess(username);
            }
            else
            {
                MessageBox.Show("Invalid username or password.");
            }

        }
        private void setup_panel_Paint(object sender, PaintEventArgs e)
        {
            bool isAdminExists = Setup.AdminExists();

            if (isAdminExists)
            {
                Setup.AdminExists();
                isAdminExists = true;
                setup_panel.Visible = false;

                login_panel.Visible = true;
                home_lbl.ForeColor = Color.DarkGoldenrod;
            }
            else
            {
                setup_panel.Visible = true;
            }
        }
        private void setup_btn_Click(object sender, EventArgs e)
        {
            string createUser = create_usertxt.Text;
            string createPass = create_passtxt.Text;
            string passVerify = verifyPass_txt.Text;
            string contact = emailPhone_txt.Text;

            if (string.IsNullOrEmpty(createUser) || string.IsNullOrEmpty(createPass) || string.IsNullOrEmpty(passVerify) || string.IsNullOrEmpty(contact))
            {
                MessageBox.Show("All requirments must be filled!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                create_usertxt.Clear();
                create_passtxt.Clear();
                verifyPass_txt.Clear();
                emailPhone_txt.Clear();
            }
            else if (createPass != passVerify)
            {
                MessageBox.Show("Verification Password not match!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                if (createUser.Contains("_Cashier") || createUser.Contains("_Owner"))
                {
                    MessageBox.Show("Setup Successfully!");
                    Setup.CreateAdminUser(createUser, passVerify, contact);
                    setup_panel.Visible = false;
                    login_panel.Visible = true;
                    home_lbl.ForeColor = Color.DarkGoldenrod;
                }
                else
                {
                    MessageBox.Show("Username must contain\nNAME + SEPARATOR + ROLE\n\nEx. Format: Juan_Cashier/Owner", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }

        private void dash_panel_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.DarkGray, 5))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawRectangle(pen, 0, 0, dash_panel.Width, dash_panel.Height);
            }
        }
        private void Dashboard_panel_Click_1(object sender, EventArgs e)
        {
            dash_panel.Visible = true;
            Menu_panel1.Visible = false;

            LoadStockOverview();
        }

        private void LoadStockOverview()
        {
            stock_dataGrid.DataSource = Dashboard.GetStockOverview();

            stock_dataGrid.ReadOnly = true;
            stock_dataGrid.AllowUserToAddRows = false;
            stock_dataGrid.AllowUserToDeleteRows = false;
            stock_dataGrid.AllowUserToResizeRows = false;
            stock_dataGrid.AllowUserToResizeColumns = false;

            stock_dataGrid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            stock_dataGrid.MultiSelect = false;

            stock_dataGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            stock_dataGrid.Columns["DateTIME"].HeaderText = "Date / Time";
            stock_dataGrid.Columns["Product_Name"].HeaderText = "Product Name";
            stock_dataGrid.Columns["Product_Details"].HeaderText = "Product Details";
            stock_dataGrid.Columns["Stock"].HeaderText = "Stock";

            stock_dataGrid.Columns["DateTIME"]
                .DefaultCellStyle.Format = "MM/dd/yyyy hh:mm tt";
        }

        private void inventory_dataGrid_DefaultValuesNeeded(object? sender, DataGridViewRowEventArgs e)
        {
            e.Row.Cells["Category"].Value = "";
            e.Row.Cells["Product_Name"].Value = "";
            e.Row.Cells["Product_Details"].Value = "";
            e.Row.Cells["Stock"].Value = 0;
            e.Row.Cells["Price"].Value = 0;
        }

        private void stock_dataGrid_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;

            if (e.CellStyle == null)
                return;

            if (stock_dataGrid.Columns[e.ColumnIndex].Name != "Stock")
                return;

            if (!int.TryParse(
                stock_dataGrid.Rows[e.RowIndex]
                    .Cells["Stock"]
                    .Value?.ToString(),
                out int stock))
            {
                return;
            }

            if (stock <= 50)
            {
                e.CellStyle.BackColor = Color.Red;
                e.CellStyle.ForeColor = Color.White;
            }
            else if (stock <= 100)
            {
                e.CellStyle.BackColor = Color.Orange;
                e.CellStyle.ForeColor = Color.Black;
            }
            else
            {
                e.CellStyle.BackColor = Color.Green;
                e.CellStyle.ForeColor = Color.White;
            }

        }

        public void LoadData()
        {
            int? selectedId = null;
            if (inventory_dataGrid.SelectedRows.Count > 0)
            {
                selectedId = Convert.ToInt32(inventory_dataGrid.SelectedRows[0].Cells["Id"].Value);
            }
            try
            {
                using (var conn = _db.GetConnection())
                {
                    conn.Open();
                    var cmd = conn.CreateCommand();
                    cmd.CommandText = "SELECT * FROM Inventory";
                    DataTable dt = new DataTable();
                    using (var reader = cmd.ExecuteReader())
                    {
                        dt.Load(reader);
                    }

                    inventory_dataGrid.DataSource = dt;

                    if (inventory_dataGrid.Columns.Contains("DateTIME"))
                    {
                        inventory_dataGrid.Columns["DateTIME"]
                            .DefaultCellStyle.Format = "MM/dd/yyyy hh:mm tt";
                    }

                    inventory_dataGrid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                }
                if (selectedId.HasValue)
                {
                    foreach (DataGridViewRow row in inventory_dataGrid.Rows)
                    {
                        if (Convert.ToInt32(row.Cells["Id"].Value) == selectedId.Value)
                        {
                            row.Selected = true;
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Occur: {ex.Message}", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }

        private void invent_panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void add_btn_Click(object sender, EventArgs e)
        {
            var selectedRow = inventory_dataGrid.CurrentRow;

            string category = selectedRow.Cells["Category"].Value?.ToString() ?? "";
            string prodName = selectedRow.Cells["Product_Name"].Value?.ToString() ?? "";
            string prodDetails = selectedRow.Cells["Product_Details"].Value?.ToString() ?? "";

            int stock;
            if (!int.TryParse(selectedRow.Cells["Stock"].Value?.ToString(), out stock))
            {
                MessageBox.Show("Invalid Stock Input");
                return;
            }

            double price;
            if (!double.TryParse(selectedRow.Cells["Price"].Value?.ToString(), out price))
            {
                MessageBox.Show("Invalid Price Input");
                return;
            }

            if (string.IsNullOrWhiteSpace(category) || string.IsNullOrWhiteSpace(prodName) || string.IsNullOrWhiteSpace(prodDetails))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }
            Inventory.AddNewItem(category, prodName, prodDetails, stock, price);
            LoadData();
            LoadSaleProducts();
        }

        private void update_btn_Click(object sender, EventArgs e)
        {
            if (inventory_dataGrid.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select a row to update.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            DataGridViewRow row = inventory_dataGrid.SelectedRows[0];

            int id = Convert.ToInt32(row.Cells["Id"].Value);

            string category = row.Cells["Category"].Value?.ToString() ?? "";
            string prodName = row.Cells["Product_Name"].Value?.ToString() ?? "";
            string prodDetails = row.Cells["Product_Details"].Value?.ToString() ?? "";

            if (!int.TryParse(row.Cells["Stock"].Value?.ToString(), out int stock))
            {
                MessageBox.Show("Invalid Stock Input.");
                return;
            }

            if (!double.TryParse(row.Cells["Price"].Value?.ToString(), out double price))
            {
                MessageBox.Show("Invalid Price Input.");
                return;
            }

            if (string.IsNullOrWhiteSpace(category) ||
                string.IsNullOrWhiteSpace(prodName) ||
                string.IsNullOrWhiteSpace(prodDetails))
            {
                MessageBox.Show("Please fill in all fields.");
                return;
            }

            Inventory.UpdateInventory(
                id,
                category,
                prodName,
                prodDetails,
                stock,
                price
            );

            MessageBox.Show("Inventory updated successfully.");

            LoadData();
            LoadSaleProducts();
        }


        private void delete_btn_Click(object sender, EventArgs e)
        {
            if (inventory_dataGrid.SelectedRows.Count > 0)
            {
                int idToDelete = Convert.ToInt32(inventory_dataGrid.SelectedRows[0].Cells["Id"].Value);
                Inventory.DeleteInventory(idToDelete);
                LoadData();
                LoadSaleProducts();
            }
            else
            {
                MessageBox.Show("Choose an Id that you want to delete.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void search_btn_Click(object sender, EventArgs e)
        {
            string searchText = search_txt.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadData();
                return;
            }

            DataTable result = Inventory.SearchInventory(searchText);

            if (result.Rows.Count == 0)
            {
                MessageBox.Show(
                    $"There's no record found for: \"{searchText.ToUpper()}\". Try again.",
                    "Search Result",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                return;
            }

            inventory_dataGrid.DataSource = result;

            inventory_dataGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
        }
        private void Inventory_panel_Click_1(object sender, EventArgs e)
        {
            invent_panel.Visible = true;
            Menu_panel1.Visible = false;
        }

        private void sales_panelboard_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Silver, 5))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawRectangle(pen, 0, 0, sales_panelboard.Width, sales_panelboard.Height);
            }
        }

        private void sale_dataGrid_Paint_1(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Black, 2))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawRectangle(pen, 0, 0, sale_dataGrid.Width, sale_dataGrid.Height);
            }
        }

        private void searchItem_txt_TextChanged(object sender, EventArgs e)
        {
            string searchText = searchItem_txt.Text.Trim();

            if (string.IsNullOrWhiteSpace(searchText))
            {
                LoadSaleProducts();
                return;
            }

            sale_dataGrid.DataSource =
                Sales.SearchProducts(searchText);

            sale_dataGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            sale_dataGrid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            sale_dataGrid.MultiSelect = false;

            if (sale_dataGrid.Columns.Contains("Id"))
            {
                sale_dataGrid.Columns["Id"].Visible = false;
            }
        }

        private void AddToSaleSummary()
        {
            if (sale_dataGrid.CurrentRow == null)
            {
                MessageBox.Show("Please select a product first.");
                return;
            }

            if (!int.TryParse(quantity_txt.Text, out int quantity) || quantity <= 0)
            {
                MessageBox.Show("Please enter a valid quantity.");
                return;
            }

            if (!int.TryParse(
                sale_dataGrid.CurrentRow.Cells["Id"].Value?.ToString(),
                out int productId))
            {
                MessageBox.Show("Invalid product ID.");
                return;
            }

            string productName =
                sale_dataGrid.CurrentRow.Cells["Product_Name"].Value?.ToString() ?? "";

            string details =
                sale_dataGrid.CurrentRow.Cells["Product_Details"].Value?.ToString() ?? "";

            if (!double.TryParse(
                sale_dataGrid.CurrentRow.Cells["Price"].Value?.ToString(),
                out double price))
            {
                MessageBox.Show("Invalid product price.");
                return;
            }

            if (!int.TryParse(
                sale_dataGrid.CurrentRow.Cells["Stock"].Value?.ToString(),
                out int stock))
            {
                MessageBox.Show("Invalid stock value.");
                return;
            }

            if (quantity > stock)
            {
                MessageBox.Show(
                    $"Insufficient stock.\nAvailable stock: {stock}",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            double total = price * quantity;

            int product_Id = Convert.ToInt32(
     sale_dataGrid.CurrentRow.Cells["Id"].Value
 );

            sale_Summary.Rows.Add(
                productId,
                productName,
                details,
                quantity,
                price,
                total
            );

            CalculateGrandTotal();
            quantity_txt.Clear();
        }

        private double CalculateGrandTotal()
        {
            double grandTotal = 0;

            foreach (DataGridViewRow row in sale_Summary.Rows)
            {
                if (row.IsNewRow)
                    continue;

                if (double.TryParse(
                    row.Cells["Total"].Value?.ToString(),
                    out double total))
                {
                    grandTotal += total;
                }
            }

            totalAmount_lbl.Text = $"₱{grandTotal:N2}";

            CalculateChange(grandTotal);

            return grandTotal;
        }

        private void CalculateChange(double grandTotal)
        {
            if (!double.TryParse(
                receiveAmt_txt.Text,
                out double receivedAmount))
            {
                receiveAmt_txt.Text = "₱0.00";
                return;
            }

            double change = receivedAmount - grandTotal;

            if (change < 0)
            {
                receiveAmt_txt.Text = "Insufficient";
                return;
            }

            TotalChange_txt.Text = $"₱{change:N2}";
        }

        private void purchaseItem_btn_Click(object sender, EventArgs e)
        {
            AddToSaleSummary();
        }

        private void LoadSaleProducts()
        {
            sale_dataGrid.DataSource = Sales.GetProducts();

            sale_dataGrid.ReadOnly = true;

            sale_dataGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;

            sale_dataGrid.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;

            sale_dataGrid.MultiSelect = false;

            if (sale_dataGrid.Columns.Contains("Id"))
            {
                sale_dataGrid.Columns["Id"].Visible = false;
            }
        }

        private void remove_btn_Click(object sender, EventArgs e)
        {
            if (sale_Summary.SelectedRows.Count == 0)
            {
                MessageBox.Show(
                    "Please select an item to remove.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            sale_Summary.Rows.RemoveAt(
                sale_Summary.SelectedRows[0].Index
            );

            CalculateGrandTotal();
        }
        private void SetupSaleSummary()
        {
            sale_Summary.Columns.Clear();

            sale_Summary.Columns.Add("Id", "Id");
            sale_Summary.Columns.Add("Product", "Product");
            sale_Summary.Columns.Add("Details", "Details");
            sale_Summary.Columns.Add("Quantity", "Quantity");
            sale_Summary.Columns.Add("Price", "Price");
            sale_Summary.Columns.Add("Total", "Total");

            sale_Summary.Columns["Id"].Visible = false;

            sale_Summary.AllowUserToAddRows = false;
            sale_Summary.ReadOnly = true;
            sale_Summary.SelectionMode =
                DataGridViewSelectionMode.FullRowSelect;
            sale_Summary.MultiSelect = false;

            sale_Summary.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void printDocPOS_PrintPage(
    object sender,
    System.Drawing.Printing.PrintPageEventArgs e)
        {
            Graphics? g = e.Graphics;

            if (g == null)
                return;

            using Font storeFont =
                new Font("Arial", 12, FontStyle.Bold);

            using Font titleFont =
                new Font("Arial", 10, FontStyle.Bold);

            using Font normalFont =
                new Font("Arial", 8);

            using Font boldFont =
                new Font("Arial", 8, FontStyle.Bold);

            using Font developerFont =
                new Font("Arial", 7);

            using Font developerBoldFont =
                new Font("Arial", 7, FontStyle.Bold);

            float pageWidth = e.PageBounds.Width;

            float left = 10;
            float right = pageWidth - 10;

            float contentWidth = right - left;

            float y = 20;


            string storeName =
                AppName.GetAppName();

            string storeAddress =
                AppName.GetAddress();

            string storeContact =
                AppName.GetOriginalAdminContact();


            DrawCenteredText(
                g,
                pageWidth,
                storeName,
                storeFont,
                y
            );

            y += 22;


            if (!string.IsNullOrWhiteSpace(storeAddress))
            {
                DrawCenteredText(
                    g,
                    pageWidth,
                    storeAddress,
                    normalFont,
                    y
                );

                y += 18;
            }


            if (!string.IsNullOrWhiteSpace(storeContact))
            {
                DrawCenteredText(
                    g,
                    pageWidth,
                    storeContact,
                    normalFont,
                    y
                );

                y += 22;
            }
            else
            {
                y += 5;
            }

            g.DrawString(
                "--------------------------------",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += 18;


            DrawCenteredText(
                g,
                pageWidth,
                "SALES RECEIPT",
                titleFont,
                y
            );

            y += 18;


            g.DrawString(
                "--------------------------------",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += 20;

            DateTime saleDateTime = DateTime.Now;


            g.DrawString(
                $"Date : {saleDateTime:MM/dd/yyyy}",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += 17;


            g.DrawString(
                $"Time : {saleDateTime:hh:mm tt}",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += 20;


            g.DrawString(
                "--------------------------------",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += 18;

            float productX = left;

            float qtyX;
            float totalX;

            if (contentWidth <= 220)
            {

                qtyX = left + contentWidth * 0.62f;
                totalX = left + contentWidth * 0.76f;
            }
            else
            {

                qtyX = left + contentWidth * 0.70f;
                totalX = left + contentWidth * 0.84f;
            }


            float productWidth =
                qtyX - productX - 5;

            g.DrawString(
                "PRODUCT",
                boldFont,
                Brushes.Black,
                productX,
                y
            );


            g.DrawString(
                "QTY",
                boldFont,
                Brushes.Black,
                qtyX,
                y
            );


            g.DrawString(
                "TOTAL",
                boldFont,
                Brushes.Black,
                totalX,
                y
            );


            y += 18;


            g.DrawString(
                "--------------------------------",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += 18;

            foreach (DataGridViewRow row in sale_Summary.Rows)
            {
                if (row.IsNewRow)
                    continue;


                string product =
                    row.Cells["Product"].Value?.ToString() ?? "";


                string details =
                    row.Cells["Details"].Value?.ToString() ?? "";


                string quantity =
                    row.Cells["Quantity"].Value?.ToString() ?? "0";


                double.TryParse(
                    row.Cells["Price"].Value?.ToString(),
                    out double price
                );


                double.TryParse(
                    row.Cells["Total"].Value?.ToString(),
                    out double total
                );

                RectangleF productArea =
                    new RectangleF(
                        productX,
                        y,
                        productWidth,
                        35
                    );


                using StringFormat productFormat =
                    new StringFormat();


                productFormat.Alignment =
                    StringAlignment.Near;

                productFormat.LineAlignment =
                    StringAlignment.Near;

                productFormat.Trimming =
                    StringTrimming.Word;


                g.DrawString(
                    product,
                    normalFont,
                    Brushes.Black,
                    productArea,
                    productFormat
                );

                g.DrawString(
                    quantity,
                    normalFont,
                    Brushes.Black,
                    qtyX,
                    y
                );


                string totalText =
                    $"₱{total:N2}";


                g.DrawString(
                    totalText,
                    normalFont,
                    Brushes.Black,
                    totalX,
                    y
                );

                SizeF productSize =
                    g.MeasureString(
                        product,
                        normalFont,
                        (int)productWidth
                    );


                float productHeight =
                    Math.Max(
                        15,
                        productSize.Height
                    );


                y += productHeight + 2;


                if (!string.IsNullOrWhiteSpace(details))
                {
                    RectangleF detailsArea =
                        new RectangleF(
                            productX,
                            y,
                            productWidth,
                            30
                        );


                    using StringFormat detailsFormat =
                        new StringFormat();


                    detailsFormat.Alignment =
                        StringAlignment.Near;

                    detailsFormat.LineAlignment =
                        StringAlignment.Near;

                    detailsFormat.Trimming =
                        StringTrimming.Word;


                    string detailsText =
                        $"   {details}";


                    g.DrawString(
                        detailsText,
                        normalFont,
                        Brushes.Black,
                        detailsArea,
                        detailsFormat
                    );


                    SizeF detailsSize =
                        g.MeasureString(
                            detailsText,
                            normalFont,
                            (int)productWidth
                        );


                    y += Math.Max(
                        15,
                        detailsSize.Height
                    ) + 3;
                }


                y += 8;
            }

            g.DrawString(
                "--------------------------------",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += 20;

            double grandTotal =
                CalculateGrandTotal();


            g.DrawString(
                "TOTAL",
                boldFont,
                Brushes.Black,
                left,
                y
            );


            string grandTotalText =
                $"₱{grandTotal:N2}";


            SizeF grandTotalSize =
                g.MeasureString(
                    grandTotalText,
                    boldFont
                );


            g.DrawString(
                grandTotalText,
                boldFont,
                Brushes.Black,
                right - grandTotalSize.Width,
                y
            );


            y += 20;

            double receivedAmount = 0;


            double.TryParse(
                cash_txt.Text,
                out receivedAmount
            );


            g.DrawString(
                "CASH",
                normalFont,
                Brushes.Black,
                left,
                y
            );


            string receivedText =
                $"₱{receivedAmount:N2}";


            SizeF receivedSize =
                g.MeasureString(
                    receivedText,
                    normalFont
                );


            g.DrawString(
                receivedText,
                normalFont,
                Brushes.Black,
                right - receivedSize.Width,
                y
            );


            y += 18;

            double changeAmount =
                receivedAmount - grandTotal;


            g.DrawString(
                "CHANGE",
                boldFont,
                Brushes.Black,
                left,
                y
            );


            string changeText =
                $"₱{changeAmount:N2}";


            SizeF changeSize =
                g.MeasureString(
                    changeText,
                    boldFont
                );


            g.DrawString(
                changeText,
                boldFont,
                Brushes.Black,
                right - changeSize.Width,
                y
            );


            y += 20;

            g.DrawString(
                "--------------------------------",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += 28;

            DrawCenteredText(
                g,
                pageWidth,
                "THANK YOU!",
                boldFont,
                y
            );

            y += 18;


            DrawCenteredText(
                g,
                pageWidth,
                "PLEASE COME AGAIN",
                normalFont,
                y
            );

            y += 28;

            g.DrawString(
                "--------------------------------",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += 20;


            DrawCenteredText(
                g,
                pageWidth,
                "iTipid_myPOS",
                titleFont,
                y
            );

            y += 18;


            DrawCenteredText(
                g,
                pageWidth,
                "Point of Sale System",
                normalFont,
                y
            );

            y += 25;


            DrawCenteredText(
                g,
                pageWidth,
                "Developed by",
                developerFont,
                y
            );

            y += 14;


            DrawCenteredText(
                g,
                pageWidth,
                "Geoven Rei Nicolas",
                developerBoldFont,
                y
            );

            y += 14;


            DrawCenteredText(
                g,
                pageWidth,
                "+639676856019",
                developerFont,
                y
            );

            y += 14;


            DrawCenteredText(
                g,
                pageWidth,
                "estonidorei@gmail.com",
                developerFont,
                y
            );

            y += 20;


            g.DrawString(
                "--------------------------------",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            e.HasMorePages = false;
        }


        private void DrawCenteredText(
            Graphics g,
            float pageWidth,
            string text,
            Font font,
            float yPosition)
        {
            SizeF textSize =
                g.MeasureString(
                    text,
                    font
                );

            float x =
                (pageWidth - textSize.Width) / 2;


            g.DrawString(
                text,
                font,
                Brushes.Black,
                x,
                yPosition
            );
        }

        private void print_btn_Click(object sender, EventArgs e)
        {
            if (sale_Summary.Rows.Count == 0)
            {
                MessageBox.Show(
                    "There are no items to print.",
                    "Warning",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (!double.TryParse(cash_txt.Text, out double received))
            {
                MessageBox.Show(
                    "Please enter a valid payment amount.",
                    "Invalid Payment",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            double grandTotal = CalculateGrandTotal();
            currentSale_lbl.Text = $"₱{grandTotal:N2}";

            if (received < grandTotal)
            {
                MessageBox.Show(
                    "Insufficient payment.",
                    "Payment Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            try
            {
                foreach (DataGridViewRow row in sale_Summary.Rows)
                {
                    if (row.IsNewRow)
                        continue;

                    int productId = Convert.ToInt32(
                        row.Cells["Id"].Value
                    );

                    int quantity = Convert.ToInt32(
                        row.Cells["Quantity"].Value
                    );

                    DataTable currentProducts =
                        Sales.GetProducts();

                    DataRow[] matchingProducts =
                        currentProducts.Select(
                            $"Id = {productId}"
                        );

                    if (matchingProducts.Length == 0)
                    {
                        MessageBox.Show(
                            "One of the products no longer exists.",
                            "Transaction Error",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );

                        return;
                    }

                    int currentStock =
                        Convert.ToInt32(
                            matchingProducts[0]["Stock"]
                        );

                    if (quantity > currentStock)
                    {
                        MessageBox.Show(
                            $"Insufficient stock for " +
                            $"{matchingProducts[0]["Product_Name"]}.\n\n" +
                            $"Available stock: {currentStock}\n" +
                            $"Requested: {quantity}",
                            "Insufficient Stock",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Warning
                        );

                        LoadSaleProducts();
                        return;
                    }
                }

                bool printEnabled = AppName.GetPrintEnabled();

                if (printEnabled)
                {
                    if (printDialogPOS.ShowDialog() != DialogResult.OK)
                    {
                        return;
                    }

                    printDocPOS.PrinterSettings =
                        printDialogPOS.PrinterSettings;
                }

                foreach (DataGridViewRow row in sale_Summary.Rows)
                {
                    if (row.IsNewRow)
                        continue;

                    int productId = Convert.ToInt32(
                        row.Cells["Id"].Value
                    );

                    int quantity = Convert.ToInt32(
                        row.Cells["Quantity"].Value
                    );

                    Inventory.DeductStock(
                        productId,
                        quantity
                    );
                }

                Sales.AddSale(grandTotal);

                if (printEnabled)
                {
                    printDocPOS.Print();
                }

                LoadSaleProducts();
                LoadData();
                LoadStockOverview();
                LoadTodaysSales();

                sale_Summary.Rows.Clear();

                cash_txt.Clear();

                receiveAmt_txt.Text = "₱0.00";
                TotalChange_txt.Text = "₱0.00";

                MessageBox.Show(
                    "Transaction completed successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                cash_txt.Enabled = false;
                print_btn.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Transaction failed:\n\n{ex.Message}",
                    "Transaction Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void Sales_panel_Click(object sender, EventArgs e)
        {
            sales_panelboard.Visible = true;
            Menu_panel1.Visible = false;
        }

        private void PosSystem_Resize(object sender, EventArgs e)
        {
            if (!_layoutReady)
                return;

            ResizeEntireUI();
        }
        protected override void OnDpiChanged(DpiChangedEventArgs e)
        {
            base.OnDpiChanged(e);

            if (!_layoutReady)
                return;

            ResizeEntireUI();
        }

        private void ResizeEntireUI()
        {
            if (!_layoutReady)
                return;

            CenterPanel(Menu_panel1);
            CenterPanel(login_panel);
            CenterPanel(setup_panel);

            CenterPanel(dash_panel);
            CenterPanel(invent_panel);
            CenterPanel(sales_panelboard);
            CenterPanel(setting_panel);
            CenterPanel(report_panel);
            CenterPanel(help_richTextBox);


            ResizeDashboardContents();

            UpdateBrandNameLayout();
            UpdateUserOperatorLabel();


            TabMenu.Left = 0;
            TabMenu.Top = 0;
            TabMenu.Width = ClientSize.Width;
            TabMenu.Height = tabMenuExpanded
                ? expandedHeight
                : constHeight;
        }
        private void ResizeDashboardContents()
        {

            int leftMargin = 23;
            int rightMargin = 23;

            int topMargin = 125;

            int bottomMargin = 110;

            stock_dataGrid.Location = new Point(
                leftMargin,
                topMargin
            );

            stock_dataGrid.Size = new Size(
                Math.Max(
                    1,
                    dash_panel.ClientSize.Width
                    - leftMargin
                    - rightMargin
                ),
                Math.Max(
                    1,
                    dash_panel.ClientSize.Height
                    - topMargin
                    - bottomMargin
                )
            );


            panel1.Left = leftMargin;

            panel1.Top =
                dash_panel.ClientSize.Height
                - panel1.Height
                - 25;

            panel2.Left =
                dash_panel.ClientSize.Width
                - panel2.Width
                - rightMargin;

            panel2.Top =
                dash_panel.ClientSize.Height
                - panel2.Height
                - 25;

            critical_lbl.Left =
        dash_panel.ClientSize.Width
        - critical_lbl.Width
        - 25;
            critical_lbl.Top = 20;

            mod_lbl.Left =
                dash_panel.ClientSize.Width
                - mod_lbl.Width
                - 25;
            mod_lbl.Top = 50;

            good_lbl.Left =
                dash_panel.ClientSize.Width
                - good_lbl.Width
                - 25;
            good_lbl.Top = 80;
        }

        private void LoadTodaysSales()
        {
            double todaysSales = Dashboard.GetTodaysSales();

            TotalSale_lbl.Text = $"₱{todaysSales:N2}";
        }

        private void flow_panel_Paint(object sender, PaintEventArgs e)
        {

        }

        private void setting_panel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.Clear(Color.DarkSlateGray);

            using (Pen pen = new Pen(Color.Silver, 5))
            {
                pen.Alignment = PenAlignment.Inset;

                e.Graphics.DrawRectangle(
                    pen,
                    0,
                    0,
                    setting_panel.Width,
                    setting_panel.Height
                );
            }
        }

        private void StartPanelSlide(Panel panel)
        {
            if (slidingPanel != null && slidingPanel != panel)
            {
                settingTimer.Stop();
            }

            slidingPanel = panel;

            if (panel.Height <= PanelClosedHeight)
            {
                targetHeight = PanelOpenHeight;
            }
            else
            {
                targetHeight = PanelClosedHeight;
            }

            settingTimer.Start();
        }

        private void settingTimer_Tick(object sender, EventArgs e)
        {
            if (slidingPanel == null)
            {
                settingTimer.Stop();
                return;
            }

            if (slidingPanel.Height < targetHeight)
            {
                slidingPanel.Height += PanelSlideSpeed;

                if (slidingPanel.Height >= targetHeight)
                {
                    slidingPanel.Height = targetHeight;
                    settingTimer.Stop();
                }
            }
            else if (slidingPanel.Height > targetHeight)
            {
                slidingPanel.Height -= PanelSlideSpeed;

                if (slidingPanel.Height <= targetHeight)
                {
                    slidingPanel.Height = targetHeight;
                    settingTimer.Stop();
                }
            }
            else
            {
                settingTimer.Stop();
            }
        }

        private void appSlide_btn_Click(object sender, EventArgs e)
        {
            StartPanelSlide(ChangeApp_panel);
        }

        private void passSlide_btn_Click(object sender, EventArgs e)
        {
            StartPanelSlide(changePass_panel);
        }

        private void addAccountSlide_btn_Click(object sender, EventArgs e)
        {
            StartPanelSlide(addAccount_panel);
        }

        private void resetSlide_btn_Click(object sender, EventArgs e)
        {
            StartPanelSlide(reset_panel);
        }

        private void setting_lbl_Click(object sender, EventArgs e)
        {
            setting_panel.Visible = true;
            Menu_panel1.Visible = false;
            login_panel.Visible = false;
            dash_panel.Visible = false;
            invent_panel.Visible = false;
            sales_panelboard.Visible = false;
            report_panel.Visible = false;
            help_richTextBox.Visible = false;

            CenterPanel(setting_panel);

            setting_lbl.ForeColor = Color.DarkGoldenrod;
            home_lbl.ForeColor = Color.DarkSlateGray;
            reports_lbl.ForeColor = Color.DarkSlateGray;
            help_lbl.ForeColor = Color.DarkSlateGray;
        }

        private void cash_txt_TextChanged(object sender, EventArgs e)
        {
            double grandTotal = CalculateGrandTotal();

            if (double.TryParse(cash_txt.Text, out double received))
            {
                receiveAmt_txt.Text = $"₱{received:N2}";

                double change = received - grandTotal;

                if (change >= 0)
                {
                    TotalChange_txt.Text = $"₱{change:N2}";
                }
                else
                {
                    TotalChange_txt.Text = "INSUFFICIENT";
                }
            }
            else
            {
                receiveAmt_txt.Text = "₱0.00";
                TotalChange_txt.Text = "₱0.00";
            }
        }
        private void cash_txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void apply_btn_Click(object sender, EventArgs e)
        {
            string newName = setApp_txt.Text.Trim();
            string newAddress = address_txt.Text.Trim();

            if (string.IsNullOrWhiteSpace(newName))
            {
                MessageBox.Show(
                    "Please enter a new app name.",
                    "Change App Name",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                setApp_txt.Focus();
                return;
            }

            if (string.IsNullOrWhiteSpace(newAddress))
            {
                MessageBox.Show(
                    "Please enter the store address.",
                    "Store Address",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                address_txt.Focus();
                return;
            }

            try
            {
                AppName.UpdateAppName(newName);

                AppName.UpdateAddress(newAddress);

                this.Text = newName;
                brandName_lbl1.Text = newName;

                MessageBox.Show(
                    "App name and address successfully saved.",
                    "Settings Updated",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                setApp_txt.Clear();

                Application.Restart();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Failed to save the settings.\n\n" +
                    ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void resetApp_btn_Click(object sender, EventArgs e)
        {
            AppName.UpdateAppName("iTinda");

            this.Text = "iTinda";
            brandName_lbl1.Text = "iTinda";
            brandName_lbl2.Text = "iTinda";

            MessageBox.Show("Reset Name Successfully!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Application.Restart();
        }
        private void UpdateUserOperatorLabel()
        {

            if (tabMenuExpanded)
            {
                UserOperator_lbl.Font = new Font(
                    UserOperator_lbl.Font.FontFamily,
                    18f,
                    UserOperator_lbl.Font.Style
                );

                UserOperator_lbl.Location = new Point(53, 82);

                indicator_online.Font = new Font(
                    indicator_online.Font.FontFamily,
                    12f,
                    FontStyle.Regular
                );

                indicator_online.Location = new Point(
                    UserOperator_lbl.Right + 6,
                    UserOperator_lbl.Top + 3
                );
            }
            else
            {
                UserOperator_lbl.Font = new Font(
                    UserOperator_lbl.Font.FontFamily,
                    12f,
                    UserOperator_lbl.Font.Style
                );

                UserOperator_lbl.Location = new Point(
                    this.ClientSize.Width
                    - UserOperator_lbl.Width
                    - indicator_online.Width
                    - 35,
                    6
                );

                indicator_online.Font = new Font(
                    indicator_online.Font.FontFamily,
                    10f,
                    FontStyle.Regular
                );

                indicator_online.Location = new Point(
                    UserOperator_lbl.Right + 5,
                    UserOperator_lbl.Top + 2
                );
            }
        }
        private void LoginSuccess(string username)
        {
            UserOperator_lbl.Text = $"👤 {username}";

            indicator_online.Text = "● Online";
            indicator_online.ForeColor = Color.DarkGreen;

            UpdateUserOperatorLabel();
        }
        private void UpdateBrandNameLayout()
        {
            if (brandName_lbl1 == null ||
                brandName_lbl2 == null ||
                myPOS_lbl == null ||
                mySetup_lbl == null)
                return;

            Control? parent1 = brandName_lbl1.Parent;
            Control? parent2 = brandName_lbl2.Parent;

            if (parent1 == null || parent2 == null)
                return;

            brandName_lbl1.AutoSize = true;
            brandName_lbl1.Size = new Size(219, 81);
            brandName_lbl1.TextAlign = ContentAlignment.MiddleCenter;

            brandName_lbl1.Left =
                (parent1.ClientSize.Width - brandName_lbl1.Width) / 2;


            brandName_lbl2.AutoSize = true;
            brandName_lbl2.Size = new Size(219, 81);
            brandName_lbl2.TextAlign = ContentAlignment.MiddleCenter;

            brandName_lbl2.Left =
                (parent2.ClientSize.Width - brandName_lbl2.Width) / 2;


            myPOS_lbl.AutoSize = true;
            mySetup_lbl.AutoSize = true;


            myPOS_lbl.Left =
                brandName_lbl1.Right - myPOS_lbl.Width + 14;

            myPOS_lbl.Top =
                brandName_lbl1.Top - myPOS_lbl.Height + 30;


            mySetup_lbl.Left =
                brandName_lbl2.Right - mySetup_lbl.Width + 14;

            mySetup_lbl.Top =
                brandName_lbl2.Top - mySetup_lbl.Height + 18;


            if (myPOS_lbl.Right > parent1.ClientSize.Width)
            {
                myPOS_lbl.Left =
                    parent1.ClientSize.Width - myPOS_lbl.Width;
            }

            if (mySetup_lbl.Right > parent2.ClientSize.Width)
            {
                mySetup_lbl.Left =
                    parent2.ClientSize.Width - mySetup_lbl.Width;
            }


            if (myPOS_lbl.Left < 0)
                myPOS_lbl.Left = 0;

            if (mySetup_lbl.Left < 0)
                mySetup_lbl.Left = 0;

        }

        private void otpCode_txt_TextChanged(object? sender, EventArgs e)
        {
            string enteredOTP = otpCode_txt.Text.Trim();

            if (enteredOTP.Length != 6)
            {
                return;
            }

            if (OTP.Verify(enteredOTP))
            {

                MessageBox.Show(
                    "OTP verified successfully!",
                    "Success",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                otpCode_txt.Enabled = false;
                otpCode_txt.Clear();
                otpCode_txt.Text = "OTP Verified!";
                changePass_txt.Enabled = true;
                updatePass_btn.Enabled = true;

            }
        }

        private void otpCode_txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void SendOtpEmail(string recipient, string otp)
        {
            string senderEmail = "itindamypos@gmail.com";
            string appPassword = "kahlfdrxbpwgaola";

            using (MailMessage mail = new MailMessage())
            {
                mail.From = new MailAddress(senderEmail);
                mail.To.Add(recipient);
                mail.Subject = "iTinda Password Reset OTP";
                mail.Body =
                    "Your iTinda OTP is: " + otp +
                    "\n\nThis OTP will expire in 1 minute.";

                using (SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587))
                {
                    smtp.EnableSsl = true;
                    smtp.UseDefaultCredentials = false;
                    smtp.Credentials = new NetworkCredential(
                        senderEmail,
                        appPassword
                    );

                    smtp.Send(mail);
                }
            }
        }

        private void send_btn_Click(object sender, EventArgs e)
        {
            string recipient = getContact_txt.Text.Trim();

            Otp_timer.Interval = 1000;
            Otp_timer.Start();

            otpExpire_lbl.Visible = true;
            otpExpire_lbl.Text = "OTP expires in: 1min";

            if (string.IsNullOrWhiteSpace(recipient))
            {
                MessageBox.Show("Please enter your Gmail address.");
                return;
            }

            if (!OTP.ConfirmContact(recipient))
            {
                MessageBox.Show("Invalid Contact!");
                return;
            }


            try
            {
                string otp = OTP.Generate();

                SendOtpEmail(recipient, otp);

                MessageBox.Show(
                    "OTP has been sent to your Gmail.",
                    "OTP Sent",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                otpCode_txt.Enabled = true;
                otpCode_txt.Clear();
                otpCode_txt.Focus();

                Otp_timer.Start();

            }
            catch (Exception ex)
            {
                OTP.Clear();

                MessageBox.Show(
                    "Failed to send OTP.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void Otp_timer_Tick(object sender, EventArgs e)
        {
            int remaining = OTP.GetRemainingSeconds();

            if (remaining <= 0)
            {
                Otp_timer.Stop();

                otpExpire_lbl.Text = "OTP Expired";

                otpCode_txt.Clear();
                otpCode_txt.Enabled = false;

                changePass_txt.Enabled = false;
                updatePass_btn.Enabled = false;

                OTP.Clear();

                return;
            }

            otpExpire_lbl.Text = $"OTP expires in: {remaining}secs";
        }

        private void updatePass_btn_Click(object sender, EventArgs e)
        {
            string newPassword = changePass_txt.Text;
            string enteredOtp = otpCode_txt.Text.Trim();

            if (string.IsNullOrWhiteSpace(newPassword))
            {
                MessageBox.Show("Please enter a new password.");
                return;
            }
            if (newPassword.Length < 8)
            {
                MessageBox.Show("Password must be at least 8 characters.");
                return;
            }
            if (string.IsNullOrWhiteSpace(enteredOtp))
            {
                MessageBox.Show("Please enter the OTP code.");
                return;
            }

            if (OTP.UpdatePassword(getContact_txt.Text.Trim(), newPassword))
            {
                MessageBox.Show("Password updated successfully! 🎉");
                OTP.Clear();
                Application.Restart();
            }
            else
            {
                MessageBox.Show("Failed to update password.");
            }
        }

        private void PosSystem_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F1)
            {
                addUsername_txt.Enabled = true;
                addPassword_txt.Enabled = true;
                addVpassword_txt.Enabled = true;
                addContact_txt.Enabled = true;
                AccAdd_btn.Enabled = true;
            }

            if (e.KeyCode == Keys.F2)
            {
                cash_txt.Enabled = true;
                print_btn.Enabled = true;
            }

            if (e.KeyCode == Keys.Escape)
            {
                endTransac_btn.Enabled = true;
            }

            if (e.KeyCode != Keys.F3)
                return;

            string username = username_txt.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "No user is currently logged in.",
                    "Verification",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            string recipient = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter your registered Gmail address:",
                "Admin Verification",
                ""
            ).Trim();

            if (string.IsNullOrWhiteSpace(recipient))
                return;

            if (!OTP.ConfirmContact(recipient))
            {
                MessageBox.Show(
                    "Invalid Contact!",
                    "Verification Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            try
            {
                string otp = OTP.Generate();

                SendOtpEmail(recipient, otp);

                MessageBox.Show(
                    "OTP has been sent to your Gmail.",
                    "OTP Sent",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                OTP.Clear();

                MessageBox.Show(
                    "Failed to send OTP.\n\n" + ex.Message,
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            string enteredOTP = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the 6-digit OTP sent to your Gmail:",
                "OTP Verification",
                ""
            ).Trim();

            if (string.IsNullOrWhiteSpace(enteredOTP))
            {
                OTP.Clear();
                return;
            }

            if (!OTP.Verify(enteredOTP))
            {
                MessageBox.Show(
                    "Invalid or expired OTP.\n\n" +
                    "The transaction data was NOT deleted.",
                    "Verification Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            MessageBox.Show(
                "OTP verified successfully.",
                "Verification Successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            if (Reports.ClearTransactionData())
            {
                OTP.Clear();

                MessageBox.Show(
                    "Transaction data has been cleared successfully.",
                    "Data Cleared",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                Application.Restart();
            }
            else
            {
                MessageBox.Show(
                    "Failed to clear transaction data.",
                    "Delete Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void AccAdd_btn_Click(object sender, EventArgs e)
        {
            string createUser = addUsername_txt.Text;
            string createPass = addPassword_txt.Text;
            string passVerify = addVpassword_txt.Text;
            string contact = addContact_txt.Text;

            if (string.IsNullOrEmpty(createUser) || string.IsNullOrEmpty(createPass) || string.IsNullOrEmpty(passVerify) || string.IsNullOrEmpty(contact))
            {
                MessageBox.Show("All requirments must be filled!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                create_usertxt.Clear();
                create_passtxt.Clear();
                verifyPass_txt.Clear();
                emailPhone_txt.Clear();

                return;
            }
            else if (contact.Contains("@gbox"))
            {
                MessageBox.Show("Sorry we can't process your Email provided!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (createPass != passVerify)
            {
                MessageBox.Show("Verification Password not match!", "", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            else if (createUser.Contains("_Cashier") || createUser.Contains("_Owner"))
            {
                MessageBox.Show("Setup Successfully!");
                Setup.CreateAdminUser(createUser, passVerify, contact);
                create_usertxt.Clear();
                create_passtxt.Clear();
                verifyPass_txt.Clear();
                emailPhone_txt.Clear();
                EnableAddAccount();
            }
            else
            {
                MessageBox.Show("Username must contain\nNAME + SEPARATOR + ROLE\n\nEx. Format: Juan_Cashier/Owner", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

        }
        private void EnableAddAccount()
        {
            addUsername_txt.Enabled = false;
            addPassword_txt.Enabled = false;
            addVpassword_txt.Enabled = false;
            addContact_txt.Enabled = false;
            AccAdd_btn.Enabled = false;
        }

        private void quantity_txt_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && e.KeyChar != (char)Keys.Back)
            {
                e.Handled = true;
            }
        }

        private void endTransac_btn_Click(object sender, EventArgs e)
        {
            string username = username_txt.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "No user is logged in.",
                    "Report Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            Reports.SaveTimeOut(
                username,
                "End Transaction Occurred"
            );

            DataTable latestReport =
                Reports.GetLatestDailyReport(username);

            if (latestReport.Rows.Count == 0)
            {
                MessageBox.Show(
                    "No daily report found for this user.",
                    "Report Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            report_dataGrid.DataSource = latestReport;

            DataGridViewRow foundRow =
                report_dataGrid.Rows[0];


            if (foundRow.IsNewRow)
            {
                MessageBox.Show(
                    "No valid daily report found.",
                    "Report Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }


            reportRowToPrint = foundRow;

            bool printEnabled =
                AppName.GetPrintEnabled();

            if (printEnabled)
            {
                if (printEndTransaction.ShowDialog() != DialogResult.OK)
                {
                    MessageBox.Show(
                        "Transaction was not ended because the report was not printed.",
                        "Transaction Cancelled",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );

                    return;
                }

                try
                {
                    printDocReport.PrinterSettings =
                        printEndTransaction.PrinterSettings;

                    printDocReport.Print();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Failed to print the daily report.\n\n{ex.Message}",
                        "Print Error",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error
                    );

                    return;
                }
            }

            Database_Backup.CreateBackup();


            MessageBox.Show(
                printEnabled
                    ? "Daily report printed successfully."
                    : "Transaction ended successfully.",
                "Transaction Ended",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );


            Application.Restart();
        }
        private void LoadReports()
        {
            report_dataGrid.DataSource = Reports.GetDailyReports();
            report_dataGrid.ReadOnly = true;
            report_dataGrid.AutoSizeColumnsMode =
                DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void reports_lbl_Click(object sender, EventArgs e)
        {
            Menu_panel1.Visible = false;
            setting_panel.Visible = false;
            help_richTextBox.Visible = false;
            login_panel.Visible = false;
            dash_panel.Visible = false;
            invent_panel.Visible = false;
            sales_panelboard.Visible = false;

            reports_lbl.ForeColor = Color.DarkGoldenrod;
            setting_lbl.ForeColor = Color.DarkSlateGray;
            home_lbl.ForeColor = Color.DarkSlateGray;
            help_lbl.ForeColor = Color.DarkSlateGray;
            report_panel.Visible = true;
        }

        private void PosSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            string username = username_txt.Text;

            if (!string.IsNullOrEmpty(username))
            {
                Reports.SaveTimeOut(username, "Form Closed");
            }
            else
            {
                MessageBox.Show(
                 "No user is logged in. The application will now close.",
                 "No Active Session",
                 MessageBoxButtons.OK,
                 MessageBoxIcon.Information
                );
            }
        }

        private void report_panel_Paint(object sender, PaintEventArgs e)
        {
            using (Pen pen = new Pen(Color.Silver, 5))
            {
                pen.Alignment = PenAlignment.Inset;
                e.Graphics.DrawRectangle(pen, 0, 0, report_panel.Width, report_panel.Height);
            }
        }

        private void printDocReport_PrintPage(
    object sender,
    PrintPageEventArgs e)
        {
            if (report_dataGrid.Rows.Count == 0)
                return;

            Graphics? g = e.Graphics;

            if (g == null)
                return;

            if (reportRowToPrint == null)
                return;

            using Font appNameFont =
                new Font("Arial", 14, FontStyle.Bold);

            using Font titleFont =
                new Font("Arial", 10, FontStyle.Bold);

            using Font sectionFont =
                new Font("Arial", 8, FontStyle.Bold);

            using Font normalFont =
                new Font("Arial", 8, FontStyle.Regular);

            using Font amountFont =
                new Font("Arial", 8, FontStyle.Regular);

            using Font totalFont =
                new Font("Arial", 9, FontStyle.Bold);

            using Font footerFont =
                new Font("Arial", 7, FontStyle.Italic);


            DataGridViewRow row = reportRowToPrint;

            string username =
                row.Cells["Username"].Value?.ToString() ?? "";

            string date =
                row.Cells["Date"].Value?.ToString() ?? "";

            string timeIn =
                row.Cells["TimeIn"].Value?.ToString() ?? "";

            string timeOut =
                row.Cells["TimeOut"].Value?.ToString() ?? "";

            string exitReason =
                row.Cells["ExitReason"].Value?.ToString() ?? "";


            int totalReceipts = 0;

            if (row.Cells["Receipts"].Value != null &&
                row.Cells["Receipts"].Value != DBNull.Value)
            {
                totalReceipts =
                    Convert.ToInt32(
                        row.Cells["Receipts"].Value
                    );
            }


            double totalSales = 0;

            if (row.Cells["TotalSales"].Value != null &&
                row.Cells["TotalSales"].Value != DBNull.Value)
            {
                totalSales =
                    Convert.ToDouble(
                        row.Cells["TotalSales"].Value
                    );
            }

            float pageWidth =
                e.PageBounds.Width;

            float left = 10;

            float right =
                pageWidth - 10;

            float contentWidth =
                right - left;

            float y = 15;

            float lineHeight = 17;

            string appName =
                AppName.GetAppName();

            SizeF appNameSize =
                g.MeasureString(
                    appName,
                    appNameFont
                );

            g.DrawString(
                appName,
                appNameFont,
                Brushes.Black,
                (pageWidth - appNameSize.Width) / 2,
                y
            );

            y += 25;

            string address =
                AppName.GetAddress();

            if (!string.IsNullOrWhiteSpace(address))
            {
                SizeF addressSize =
                    g.MeasureString(
                        address,
                        normalFont,
                        (int)contentWidth
                    );

                g.DrawString(
                    address,
                    normalFont,
                    Brushes.Black,
                    left + ((contentWidth - addressSize.Width) / 2),
                    y
                );

                y += 18;
            }

            string reportTitle =
                "DAILY SALES REPORT";

            SizeF reportTitleSize =
                g.MeasureString(
                    reportTitle,
                    titleFont
                );

            g.DrawString(
                reportTitle,
                titleFont,
                Brushes.Black,
                (pageWidth - reportTitleSize.Width) / 2,
                y
            );

            y += 25;

            g.DrawLine(
                Pens.Black,
                left,
                y,
                right,
                y
            );

            y += 12;
            g.DrawString(
                "SESSION INFORMATION",
                sectionFont,
                Brushes.Black,
                left,
                y
            );

            y += 20;


            g.DrawString(
                $"Username : {username}",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += lineHeight;


            g.DrawString(
                $"Date     : {date}",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += lineHeight;


            g.DrawString(
                $"Time In  : {timeIn}",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += lineHeight;


            g.DrawString(
                $"Time Out : {timeOut}",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += lineHeight;


            g.DrawString(
                $"Reason   : {exitReason}",
                normalFont,
                Brushes.Black,
                left,
                y
            );

            y += 12;


            g.DrawLine(
                Pens.Black,
                left,
                y,
                right,
                y
            );

            y += 12;

            string accountabilityTitle =
                "ACCOUNTABILITY SUMMARY";

            SizeF accountabilitySize =
                g.MeasureString(
                    accountabilityTitle,
                    sectionFont
                );

            g.DrawString(
                accountabilityTitle,
                sectionFont,
                Brushes.Black,
                (pageWidth - accountabilitySize.Width) / 2,
                y
            );

            y += 22;

            g.DrawString(
                "Receipt #",
                sectionFont,
                Brushes.Black,
                left,
                y
            );


            string amountHeader =
                "Amount";

            SizeF amountHeaderSize =
                g.MeasureString(
                    amountHeader,
                    sectionFont
                );

            g.DrawString(
                amountHeader,
                sectionFont,
                Brushes.Black,
                right - amountHeaderSize.Width,
                y
            );

            y += 17;


            g.DrawLine(
                Pens.Black,
                left,
                y,
                right,
                y
            );

            y += 8;

            try
            {
                using (var conn =
                    new ConnectionDB().GetConnection())
                {
                    conn.Open();

                    var cmd =
                        conn.CreateCommand();

                    cmd.CommandText = @"
                SELECT TotalAmount
                FROM Sales
                WHERE DateTime >= @StartDate
                  AND DateTime < @EndDate
                ORDER BY Id ASC";


                    DateTime reportDate;

                    if (!DateTime.TryParse(
                        date,
                        out reportDate))
                    {
                        reportDate =
                            DateTime.Today;
                    }


                    cmd.Parameters.AddWithValue(
                        "@StartDate",
                        reportDate
                            .Date
                            .ToString(
                                "yyyy-MM-dd HH:mm:ss"
                            )
                    );


                    cmd.Parameters.AddWithValue(
                        "@EndDate",
                        reportDate
                            .Date
                            .AddDays(1)
                            .ToString(
                                "yyyy-MM-dd HH:mm:ss"
                            )
                    );


                    int receiptNumber = 1;


                    using (var reader =
                        cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            double receiptAmount =
                                Convert.ToDouble(
                                    reader.GetValue(0)
                                );


                            // Thermal paper page limit
                            if (y + 20 >
                                e.MarginBounds.Bottom - 80)
                            {
                                e.HasMorePages = true;
                                return;
                            }


                            string receiptText =
                                $"Receipt #{receiptNumber}";


                            g.DrawString(
                                receiptText,
                                normalFont,
                                Brushes.Black,
                                left,
                                y
                            );


                            string receiptAmountText =
                                $"₱{receiptAmount:N2}";


                            SizeF receiptAmountSize =
                                g.MeasureString(
                                    receiptAmountText,
                                    amountFont
                                );


                            g.DrawString(
                                receiptAmountText,
                                amountFont,
                                Brushes.Black,
                                right -
                                receiptAmountSize.Width,
                                y
                            );


                            y += lineHeight;

                            receiptNumber++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                g.DrawString(
                    "Unable to load receipt details.",
                    normalFont,
                    Brushes.Black,
                    left,
                    y
                );

                y += lineHeight;


                g.DrawString(
                    ex.Message,
                    footerFont,
                    Brushes.Black,
                    left,
                    y
                );

                y += 20;
            }


            // ==========================================
            // TOTAL LINE
            // ==========================================

            y += 5;

            g.DrawLine(
                Pens.Black,
                left,
                y,
                right,
                y
            );

            y += 14;

            g.DrawString(
                "Total Receipt",
                totalFont,
                Brushes.Black,
                left,
                y
            );


            string receiptTotalText =
                totalReceipts.ToString();


            SizeF receiptTotalSize =
                g.MeasureString(
                    receiptTotalText,
                    totalFont
                );


            g.DrawString(
                receiptTotalText,
                totalFont,
                Brushes.Black,
                right -
                receiptTotalSize.Width,
                y
            );

            y += 22;


            g.DrawString(
                "Total Sales",
                totalFont,
                Brushes.Black,
                left,
                y
            );


            string totalSalesText =
                $"₱{totalSales:N2}";


            SizeF totalSalesSize =
                g.MeasureString(
                    totalSalesText,
                    totalFont
                );


            g.DrawString(
                totalSalesText,
                totalFont,
                Brushes.Black,
                right -
                totalSalesSize.Width,
                y
            );

            y += 28;

            Rectangle totalBox =
                new Rectangle(
                    (int)left,
                    (int)y,
                    (int)contentWidth,
                    45
                );


            using (Pen boxPen =
                new Pen(Color.Black, 1.2f))
            {
                g.DrawRectangle(
                    boxPen,
                    totalBox
                );
            }


            y += 8;


            g.DrawString(
                "TOTAL SALES",
                totalFont,
                Brushes.Black,
                left + 8,
                y
            );


            string finalAmount =
                $"₱{totalSales:N2}";


            SizeF finalAmountSize =
                g.MeasureString(
                    finalAmount,
                    totalFont
                );


            g.DrawString(
                finalAmount,
                totalFont,
                Brushes.Black,
                right -
                finalAmountSize.Width -
                8,
                y
            );


            y += 40;

            g.DrawLine(
                Pens.Black,
                left,
                y,
                right,
                y
            );

            y += 12;

            string footer =
                "End of Daily Transaction Report";


            SizeF footerSize =
                g.MeasureString(
                    footer,
                    footerFont
                );


            g.DrawString(
                footer,
                footerFont,
                Brushes.Black,
                (pageWidth -
                footerSize.Width) / 2,
                y
            );


            y += 18;


            string generated =
                $"Generated: {DateTime.Now:MMM dd, yyyy hh:mm tt}";


            SizeF generatedSize =
                g.MeasureString(
                    generated,
                    footerFont
                );


            g.DrawString(
                generated,
                footerFont,
                Brushes.Black,
                (pageWidth -
                generatedSize.Width) / 2,
                y
            );


            e.HasMorePages = false;
        }

        private void LoadHelpGuide()
        {
            help_richTextBox.ReadOnly = true;
            help_richTextBox.WordWrap = true;
            help_richTextBox.ScrollBars =
                RichTextBoxScrollBars.Vertical;


            help_richTextBox.BorderStyle =
                BorderStyle.FixedSingle;

            help_richTextBox.BackColor =
                Color.DarkSlateGray;

            help_richTextBox.ForeColor =
                Color.Tan;

            help_richTextBox.Padding = new Padding(25, 10, 25, 10);

            help_richTextBox.Rtf =
                HelpContent.GetHelpGuide();

            help_richTextBox.SelectionStart = 0;
            help_richTextBox.ScrollToCaret();
        }

        private void help_lbl_Click(object sender, EventArgs e)
        {
            help_richTextBox.Visible = true;

            home_lbl.ForeColor = Color.DarkSlateGray;
            reports_lbl.ForeColor = Color.DarkSlateGray;
            setting_lbl.ForeColor = Color.DarkSlateGray;
            help_lbl.ForeColor = Color.DarkGoldenrod;

            Menu_panel1.Visible = false;
            login_panel.Visible = false;
            dash_panel.Visible = false;
            invent_panel.Visible = false;
            sales_panelboard.Visible = false;
            setting_panel.Visible = false;
            report_panel.Visible = false;

        }

        private void button1_Click(object sender, EventArgs e)
        {
            string username = username_txt.Text.Trim();

            if (string.IsNullOrWhiteSpace(username))
            {
                MessageBox.Show(
                    "No user is currently logged in.",
                    "Reset App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            int userId = Reset.GetUserId(username);

            if (userId <= 0)
            {
                MessageBox.Show(
                    "Unable to identify the currently logged-in user.",
                    "Reset App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            if (!Reset.IsOriginalSetupAccount(userId))
            {
                MessageBox.Show(
                    "Access Denied.\n\n" +
                    "Only the original setup account can reset the application.",
                    "Reset App",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            DialogResult firstConfirm = MessageBox.Show(
                "WARNING!\n\n" +
                "You are about to reset the application.\n\n" +
                "All application data will be permanently deleted.\n\n" +
                "This action cannot be undone.\n\n" +
                "Do you want to continue?",
                "Reset Application",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (firstConfirm != DialogResult.Yes)
                return;

            string recipient = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the registered Gmail address of the setup account:",
                "Admin Verification",
                ""
            ).Trim();

            if (string.IsNullOrWhiteSpace(recipient))
                return;

            if (!OTP.ConfirmContact(recipient))
            {
                MessageBox.Show(
                    "Invalid Contact!\n\n" +
                    "The Gmail address is not registered.",
                    "Verification Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );

                return;
            }

            try
            {
                string otp = OTP.Generate();

                SendOtpEmail(recipient, otp);

                MessageBox.Show(
                    "OTP has been sent to your registered Gmail address.",
                    "OTP Sent",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
            catch (Exception ex)
            {
                OTP.Clear();

                MessageBox.Show(
                    "Failed to send OTP.\n\n" +
                    ex.Message,
                    "OTP Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            string enteredOTP = Microsoft.VisualBasic.Interaction.InputBox(
                "Enter the 6-digit OTP sent to your Gmail:",
                "OTP Verification",
                ""
            ).Trim();

            if (string.IsNullOrWhiteSpace(enteredOTP))
            {
                OTP.Clear();
                return;
            }

            if (!OTP.Verify(enteredOTP))
            {
                OTP.Clear();

                MessageBox.Show(
                    "Invalid or expired OTP.\n\n" +
                    "The application was NOT reset.",
                    "Verification Failed",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );

                return;
            }

            MessageBox.Show(
                "OTP verified successfully.",
                "Verification Successful",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );

            DialogResult finalConfirm = MessageBox.Show(
                "FINAL WARNING!\n\n" +
                "ALL APPLICATION DATA WILL BE PERMANENTLY DELETED.\n\n" +
                "This action cannot be undone.\n\n" +
                "Are you absolutely sure you want to reset the application?",
                "FINAL RESET CONFIRMATION",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning
            );

            if (finalConfirm != DialogResult.Yes)
            {
                OTP.Clear();
                return;
            }

            if (Reset.ResetApplicationData())
            {
                OTP.Clear();

                MessageBox.Show(
                    "The application has been reset successfully.",
                    "Reset Complete",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );

                Application.Restart();
            }
            else
            {
                OTP.Clear();

                MessageBox.Show(
                    "The application could not be reset.",
                    "Reset Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error
                );
            }
        }

        private void print_Toggle_CheckedChanged(object sender, EventArgs e)
        {
            AppName.SetPrintEnabled(print_Toggle.Checked);
            print_lbl.Text =
                print_Toggle.Checked
                    ? "Printing Enabled"
                    : "Printing Disabled";
        }
    }
}