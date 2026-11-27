using System.Drawing;
using System.Windows.Forms;
using SaigonBite.Models;

namespace SaigonBite.UI.Views;

/// <summary>Redesigned split-screen culinary hero authentication view (BR18 single schema).</summary>
public class AuthView : UserControl
{
    private readonly DemoAppContext _ctx;
    private readonly Action<User> _onSignedIn;

    private readonly RoundedPanel _signInCard;
    private readonly RoundedPanel _registerCard;

    private readonly TextBox _txtLoginKey;
    private readonly TextBox _txtPassword;
    private readonly ComboBox _cboRole;
    private readonly Label _lblSignInErr;

    private bool _isTouristRegister;
    private readonly Button _btnTabLocal;
    private readonly Button _btnTabTourist;
    private readonly Label _lblField1;
    private readonly TextBox _txtField1;
    private readonly Label _lblField2;
    private readonly TextBox _txtField2;
    private readonly Label _lblField3;
    private readonly TextBox _txtField3;
    private readonly Label _lblField4;
    private readonly TextBox _txtField4;
    private readonly Label _lblRegPw;
    private readonly TextBox _txtRegPass;
    private readonly Label _lblOtp;
    private readonly TextBox _txtOtp;
    private readonly Button _btnSendOtp;
    private readonly Label _lblOtpSent;
    private readonly Label _lblRegErr;
    private readonly Button _btnCreate;
    private readonly LinkLabel _lnkBackSign;

    public AuthView(DemoAppContext ctx, Action<User> onSignedIn)
    {
        _ctx = ctx;
        _onSignedIn = onSignedIn;
        Dock = DockStyle.Fill;
        BackColor = SaigonBiteTheme.BgCanvas;

        // Left Split Culinary Hero Banner
        var hero = new HeroBannerPanel
        {
            BackgroundPhoto = AssetLoader.GetAuthHeroImage(),
            HorizontalScrim = false,
            CornerRadius = 0,
            Dock = DockStyle.Fill
        };

        var lblBadge = new PictureBox
        {
            Image = AssetLoader.GetHeroLogoImage(),
            SizeMode = PictureBoxSizeMode.Zoom,
            BackColor = Color.Transparent,
            Location = new Point(30, 28),
            Size = new Size(52, 42)
        };
        var lblHeroBrand = new Label
        {
            Text = "SaigonBite",
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Location = new Point(92, 26),
            AutoSize = true
        };
        var lblHeroSub = new Label
        {
            Text = LocalizationHelper.T("Cloud-Kitchen Food Delivery Hub · Ho Chi Minh City", "Nền tảng Bếp Đám Mây Đa Thương Hiệu · TP.HCM"),
            Font = SaigonBiteTheme.FontSmall,
            ForeColor = Color.Gainsboro,
            BackColor = Color.Transparent,
            Location = new Point(94, 52),
            AutoSize = true
        };
        var lblHeadline = new Label
        {
            Text = LocalizationHelper.T("Discover rich Saigon culinary flavors.", "Khám phá ẩm thực phong phú tại Sài Gòn."),
            Font = new Font("Segoe UI", 21F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Location = new Point(30, 210),
            Size = new Size(510, 96)
        };
        var lblHeroDesc = new Label
        {
            Text = LocalizationHelper.T(
                "Order across Pho 24, Bun Cha 36, Com Tam SG, Banh Mi Hoa Ma & Drinks Bar in one checkout with live kitchen load protection.",
                "Đặt chung Phở 24, Bún Chả 36, Cơm Tấm SG, Bánh Mì Hòa Mã & Drinks Bar trong một giỏ hàng với bảo vệ tải bếp trực tiếp."),
            Font = SaigonBiteTheme.FontBody,
            ForeColor = Color.WhiteSmoke,
            BackColor = Color.Transparent,
            Location = new Point(32, 314),
            Size = new Size(495, 58)
        };

        var lblHubs = new Label
        {
            Text = "● Thu Duc: 22% (Quiet Kitchen −10%)   ● District 1: 45%   ● Binh Thanh: 92% FULL",
            Font = SaigonBiteTheme.FontSmallBold,
            ForeColor = Color.White,
            BackColor = Color.FromArgb(145, 28, 25, 23),
            Location = new Point(30, 540),
            Size = new Size(515, 34),
            TextAlign = ContentAlignment.MiddleCenter
        };
        hero.Controls.Add(lblHeadline);
        Controls.Add(hero);

        // Right Split: Sign In Card
        _signInCard = SaigonBiteTheme.CreateCard(14);
        _signInCard.Size = new Size(470, 360);
        _signInCard.Location = new Point(700, 120);

        var lblTitle = new Label
        {
            Text = LocalizationHelper.T("Welcome back", "Chào mừng trở lại"),
            Font = new Font("Segoe UI", 16F, FontStyle.Bold),
            ForeColor = SaigonBiteTheme.TextPrimary,
            Location = new Point(30, 26),
            AutoSize = true
        };
        var lblSub = new Label
        {
            Text = LocalizationHelper.T("Sign in with your phone number or passport.", "Đăng nhập bằng số điện thoại hoặc hộ chiếu của bạn."),
            Font = SaigonBiteTheme.FontSmall,
            ForeColor = SaigonBiteTheme.TextMuted,
            Location = new Point(32, 58),
            AutoSize = true
        };

        var lblKey = new Label { Text = LocalizationHelper.T("Phone / Passport", "Số điện thoại / Hộ chiếu"), Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(30, 94), AutoSize = true };
        _txtLoginKey = new TextBox { Text = "", PlaceholderText = LocalizationHelper.T("Phone number, passport, or username", "Số điện thoại, hộ chiếu hoặc tài khoản"), Font = SaigonBiteTheme.FontBody, Location = new Point(30, 115), Width = 410 };

        var lblPw = new Label { Text = LocalizationHelper.T("Password", "Mật khẩu"), Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(30, 156), AutoSize = true };
        _txtPassword = new TextBox { Text = "", PlaceholderText = LocalizationHelper.T("Enter password", "Nhập mật khẩu"), UseSystemPasswordChar = true, Font = SaigonBiteTheme.FontBody, Location = new Point(30, 177), Width = 410 };

        var lblRole = new Label { Text = LocalizationHelper.T("Role", "Vai trò"), Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(30, 218), AutoSize = true };
        _cboRole = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Font = SaigonBiteTheme.FontBody, Location = new Point(30, 239), Width = 410 };
        _cboRole.Items.AddRange(new object[]
        {
            "Local Customer (0901234567)",
            "Foreign Tourist (C1234567)",
            "Kitchen Staff",
            "Shipper",
            "System Admin"
        });
        _cboRole.SelectedIndex = 0;
        _cboRole.SelectedIndexChanged += (_, _) =>
        {
            _txtLoginKey.Text = _cboRole.SelectedIndex switch
            {
                0 => "0901234567",
                1 => "C1234567",
                2 => "kitchen.thuduc",
                3 => "shipper.thuduc",
                _ => "admin"
            };
        };

        var btnSignIn = SaigonBiteTheme.CreatePrimaryButton(LocalizationHelper.T("Sign in", "Đăng nhập"), 410, 40);
        btnSignIn.Location = new Point(30, 224);
        btnSignIn.Click += (_, _) => DoSignIn();

        _lblSignInErr = new Label { ForeColor = SaigonBiteTheme.DangerRed, Font = SaigonBiteTheme.FontSmallBold, Location = new Point(30, 272), Size = new Size(410, 22) };

        var lnkToReg = new LinkLabel
        {
            Text = LocalizationHelper.T("No account yet? Register →", "Chưa có tài khoản? Đăng ký →"),
            Font = SaigonBiteTheme.FontSmallBold,
            LinkColor = SaigonBiteTheme.AccentOrange,
            LinkBehavior = LinkBehavior.NeverUnderline,
            Location = new Point(30, 304),
            AutoSize = true
        };
        lnkToReg.Click += (_, _) => ToggleMode(register: true);

        _signInCard.Controls.AddRange(new Control[] { lblTitle, lblSub, lblKey, _txtLoginKey, lblPw, _txtPassword, btnSignIn, _lblSignInErr, lnkToReg });

        // Right Split: Register Card (Fixed tab order: Local | Tourist)
        _registerCard = SaigonBiteTheme.CreateCard(14);
        _registerCard.Size = new Size(470, 560);
        _registerCard.Location = new Point(700, 48);
        _registerCard.Visible = false;

        var lblRegTitle = new Label { Text = LocalizationHelper.T("Create account", "Tạo tài khoản"), Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = SaigonBiteTheme.TextPrimary, Location = new Point(30, 22), AutoSize = true };
        _btnTabLocal = SaigonBiteTheme.CreatePillButton(LocalizationHelper.T("Local Customer", "Khách địa phương"), true, 138, 30);
        _btnTabLocal.Location = new Point(30, 60);
        _btnTabLocal.Click += (_, _) => SetRegisterTab(false);

        _btnTabTourist = SaigonBiteTheme.CreatePillButton(LocalizationHelper.T("Foreign Tourist", "Khách nước ngoài"), false, 146, 30);
        _btnTabTourist.Location = new Point(176, 60);
        _btnTabTourist.Click += (_, _) => SetRegisterTab(true);

        _lblField1 = new Label { Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(30, 102), AutoSize = true };
        _txtField1 = new TextBox { Font = SaigonBiteTheme.FontBody, Location = new Point(30, 121), Width = 410 };

        _lblField2 = new Label { Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(30, 155), AutoSize = true };
        _txtField2 = new TextBox { Font = SaigonBiteTheme.FontBody, Location = new Point(30, 174), Width = 410 };

        _lblField3 = new Label { Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(30, 208), AutoSize = true };
        _txtField3 = new TextBox { Font = SaigonBiteTheme.FontBody, Location = new Point(30, 227), Width = 410 };

        _lblField4 = new Label { Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(30, 261), AutoSize = true };
        _txtField4 = new TextBox { Font = SaigonBiteTheme.FontBody, Location = new Point(30, 280), Width = 410 };

        _lblRegPw = new Label { Text = LocalizationHelper.T("Password (min 8 characters)", "Mật khẩu (tối thiểu 8 ký tự)"), Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(30, 314), AutoSize = true };
        _txtRegPass = new TextBox { Text = "", PlaceholderText = LocalizationHelper.T("At least 8 characters", "Tối thiểu 8 ký tự"), UseSystemPasswordChar = true, Font = SaigonBiteTheme.FontBody, Location = new Point(30, 333), Width = 410 };

        _lblOtp = new Label { Text = LocalizationHelper.T("Verification code (OTP)", "Mã xác thực (OTP)"), Font = SaigonBiteTheme.FontSmallBold, ForeColor = SaigonBiteTheme.TextMuted, Location = new Point(30, 367), AutoSize = true };
        _txtOtp = new TextBox { Text = "", PlaceholderText = LocalizationHelper.T("6-digit OTP", "Nhập mã 6 số"), Font = SaigonBiteTheme.FontBody, Location = new Point(30, 386), Width = 290 };
        _btnSendOtp = SaigonBiteTheme.CreateOutlineButton(LocalizationHelper.T("Send OTP", "Gửi OTP"), 110, 28);
        _btnSendOtp.Location = new Point(330, 385);
        _btnSendOtp.Click += (_, _) => SendOtp();

        _lblOtpSent = new Label { ForeColor = SaigonBiteTheme.SuccessGreen, Font = SaigonBiteTheme.FontSmall, Location = new Point(30, 417), Size = new Size(410, 18) };
        _lblRegErr = new Label { ForeColor = SaigonBiteTheme.DangerRed, Font = SaigonBiteTheme.FontSmallBold, Location = new Point(30, 437), Size = new Size(410, 22) };

        _btnCreate = SaigonBiteTheme.CreatePrimaryButton(LocalizationHelper.T("Create account", "Tạo tài khoản"), 410, 40);
        _btnCreate.Location = new Point(30, 464);
        _btnCreate.Click += (_, _) => DoRegister();

        _lnkBackSign = new LinkLabel
        {
            Text = LocalizationHelper.T("Already registered? Sign in", "Đã có tài khoản? Đăng nhập"),
            Font = SaigonBiteTheme.FontSmallBold,
            LinkColor = SaigonBiteTheme.AccentOrange,
            LinkBehavior = LinkBehavior.NeverUnderline,
            Location = new Point(30, 516),
            AutoSize = true
        };
        _lnkBackSign.Click += (_, _) => ToggleMode(register: false);

        _registerCard.Controls.AddRange(new Control[]
        {
            lblRegTitle, _btnTabLocal, _btnTabTourist,
            _lblField1, _txtField1, _lblField2, _txtField2, _lblField3, _txtField3, _lblField4, _txtField4,
            _lblRegPw, _txtRegPass, _lblOtp, _txtOtp, _btnSendOtp, _lblOtpSent, _lblRegErr, _btnCreate, _lnkBackSign
        });

        hero.Controls.Add(_signInCard);
        hero.Controls.Add(_registerCard);
        _signInCard.BringToFront();
        _registerCard.BringToFront();
        SetRegisterTab(false);
    }

    private void ToggleMode(bool register)
    {
        _signInCard.Visible = !register;
        _registerCard.Visible = register;
        _lblSignInErr.Text = "";
        _lblOtpSent.Text = "";
        _lblOtpSent.Text = "";
        _lblRegErr.Text = "";
    }

    private void SetRegisterTab(bool tourist)
    {
        _isTouristRegister = tourist;
        _btnTabLocal.BackColor = !tourist ? SaigonBiteTheme.AccentOrange : SaigonBiteTheme.BgPillSoft;
        _btnTabLocal.ForeColor = !tourist ? Color.White : SaigonBiteTheme.AccentOrange;
        _btnTabTourist.BackColor = tourist ? SaigonBiteTheme.AccentOrange : SaigonBiteTheme.BgPillSoft;
        _btnTabTourist.ForeColor = tourist ? Color.White : SaigonBiteTheme.AccentOrange;

        if (!tourist)
        {
            _lblField1.Text = LocalizationHelper.T("Full name", "Họ và tên");
            _txtField1.Text = "";
            _txtField1.PlaceholderText = LocalizationHelper.T("e.g. Nguyen Van A", "VD: Nguyễn Văn A");
            _lblField2.Text = LocalizationHelper.T("Phone number", "Số điện thoại");
            _txtField2.Text = "";
            _txtField2.PlaceholderText = LocalizationHelper.T("e.g. 0901234567", "VD: 0901234567");
            _lblField3.Visible = _txtField3.Visible = false;
            _lblField4.Visible = _txtField4.Visible = false;

            _lblRegPw.Location = new Point(30, 208);
            _txtRegPass.Location = new Point(30, 227);
            _lblOtp.Location = new Point(30, 261);
            _txtOtp.Location = new Point(30, 280);
            _btnSendOtp.Location = new Point(330, 279);
            _lblOtpSent.Location = new Point(30, 311);
            _lblRegErr.Location = new Point(30, 331);
            _btnCreate.Location = new Point(30, 358);
            _lnkBackSign.Location = new Point(30, 410);
            _registerCard.Size = new Size(470, 456);
            _registerCard.Location = new Point(700, 96);
        }
        else
        {
            _lblField1.Text = LocalizationHelper.T("Passport", "Hộ chiếu");
            _txtField1.Text = "";
            _txtField1.PlaceholderText = LocalizationHelper.T("e.g. C1234567", "VD: C1234567");
            _lblField2.Text = LocalizationHelper.T("Nationality", "Quốc tịch");
            _txtField2.Text = "";
            _txtField2.PlaceholderText = LocalizationHelper.T("e.g. United States", "VD: United States");
            _lblField3.Text = "Email";
            _txtField3.Text = "";
            _txtField3.PlaceholderText = "john@mail.com";
            _lblField4.Text = LocalizationHelper.T("Address", "Địa chỉ");
            _txtField4.Text = "";
            _txtField4.PlaceholderText = LocalizationHelper.T("e.g. 25 Le Loi, District 1", "VD: 25 Lê Lợi, Quận 1");
            _lblField3.Visible = _txtField3.Visible = true;
            _lblField4.Visible = _txtField4.Visible = true;

            _lblRegPw.Location = new Point(30, 314);
            _txtRegPass.Location = new Point(30, 333);
            _lblOtp.Location = new Point(30, 367);
            _txtOtp.Location = new Point(30, 386);
            _btnSendOtp.Location = new Point(330, 385);
            _lblOtpSent.Location = new Point(30, 417);
            _lblRegErr.Location = new Point(30, 437);
            _btnCreate.Location = new Point(30, 464);
            _lnkBackSign.Location = new Point(30, 516);
            _registerCard.Size = new Size(470, 560);
            _registerCard.Location = new Point(700, 48);
        }
        _lblRegErr.Text = "";
    }

    private void DoSignIn()
    {
        var key = _txtLoginKey.Text.Trim();
        if (string.IsNullOrWhiteSpace(key) || string.IsNullOrEmpty(_txtPassword.Text))
        {
            _lblSignInErr.Text = LocalizationHelper.T("Please enter your account and password.", "Vui lòng nhập tài khoản và mật khẩu.");
            return;
        }
        var user = _ctx.Users.FirstOrDefault(u => string.Equals(u.Username, key, StringComparison.OrdinalIgnoreCase) && u.IsActive);
        if (user == null)
        {
            _lblSignInErr.Text = LocalizationHelper.T("Account not found.", "Không tìm thấy tài khoản.");
            return;
        }
        var hash = _ctx.AuthService.HashPassword(_txtPassword.Text);
        if (!string.Equals(hash, user.PasswordHash, StringComparison.OrdinalIgnoreCase))
        {
            _lblSignInErr.Text = LocalizationHelper.T("Incorrect password.", "Sai mật khẩu.");
            return;
        }
        _onSignedIn(user);
    }

    private void SendOtp()
    {
        _lblOtpSent.Text = "";
        _lblRegErr.Text = "";

        if (!_isTouristRegister)
        {
            var phone = _txtField2.Text.Trim();
            if (!_ctx.AuthService.ValidateVietnamesePhone(phone))
            {
                _lblRegErr.Text = LocalizationHelper.T("Phone number must be 10 digits starting with 03, 05, 07, 08, or 09.", "SĐT phải có 10 số, bắt đầu bằng 03, 05, 07, 08, 09");
                return;
            }
            if (_ctx.Users.Any(u => string.Equals(u.Username, phone, StringComparison.OrdinalIgnoreCase)))
            {
                _lblRegErr.Text = LocalizationHelper.T("Phone number is already registered.", "Số điện thoại này đã được đăng ký.");
                return;
            }
        }
        else
        {
            var passport = _txtField1.Text.Trim();
            if (!_ctx.AuthService.ValidatePassport(passport))
            {
                _lblRegErr.Text = LocalizationHelper.T("Passport number must be 6–9 letters or digits.", "Hộ chiếu phải gồm 6–9 chữ cái hoặc số.");
                return;
            }
            var email = _txtField3.Text.Trim();
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.'))
            {
                _lblRegErr.Text = LocalizationHelper.T("Please enter a valid email address.", "Vui lòng nhập địa chỉ email hợp lệ.");
                return;
            }
        }

        if (!_ctx.AuthService.ValidatePassword(_txtRegPass.Text))
        {
            _lblRegErr.Text = LocalizationHelper.T("Password must be at least 8 characters.", "Mật khẩu phải từ 8 ký tự trở lên.");
            return;
        }

        _lblOtpSent.Text = LocalizationHelper.T("Your OTP is: 123456", "OTP của bạn là: 123456");
    }

    private void DoRegister()
    {
        if (!_ctx.AuthService.ValidatePassword(_txtRegPass.Text))
        {
            _lblRegErr.Text = LocalizationHelper.T("Password must be at least 8 characters.", "Mật khẩu phải từ 8 ký tự trở lên.");
            return;
        }
        if (_txtOtp.Text.Trim() != "123456")
        {
            _lblRegErr.Text = LocalizationHelper.T("Invalid OTP.", "Mã OTP không hợp lệ.");
            return;
        }
        var hash = _ctx.AuthService.HashPassword(_txtRegPass.Text);
        if (!_isTouristRegister)
        {
            var fullName = _txtField1.Text.Trim();
            if (string.IsNullOrWhiteSpace(fullName))
            {
                _lblRegErr.Text = LocalizationHelper.T("Please enter your full name.", "Vui lòng nhập họ và tên.");
                return;
            }
            var phone = _txtField2.Text.Trim();
            if (!_ctx.AuthService.ValidateVietnamesePhone(phone))
            {
                _lblRegErr.Text = LocalizationHelper.T("Phone number must be 10 digits starting with 03, 05, 07, 08, or 09.", "SĐT phải có 10 số, bắt đầu bằng 03, 05, 07, 08, 09");
                return;
            }
            if (_ctx.Users.Any(u => string.Equals(u.Username, phone, StringComparison.OrdinalIgnoreCase)))
            {
                _lblRegErr.Text = LocalizationHelper.T("Phone number is already registered.", "Số điện thoại này đã được đăng ký.");
                return;
            }
            var newUser = new User { UserID = _ctx.Users.Count + 1, Username = phone, FullName = fullName, PasswordHash = hash, Role = UserRole.LocalCustomer };
            _ctx.Users.Add(newUser);
            _onSignedIn(newUser);
        }
        else
        {
            var passport = _txtField1.Text.Trim();
            if (!_ctx.AuthService.ValidatePassport(passport))
            {
                _lblRegErr.Text = LocalizationHelper.T("Passport number must be 6–9 letters or digits.", "Hộ chiếu phải gồm 6–9 chữ cái hoặc số.");
                return;
            }
            var newUser = new User { UserID = _ctx.Users.Count + 1, Username = passport, FullName = passport, PasswordHash = hash, Role = UserRole.ForeignTourist };
            _ctx.Users.Add(newUser);
            _onSignedIn(newUser);
        }
    }
}
