using System;
using System.Windows.Forms;

public class LoginForm : Form
{
    private TextBox tbToken;
    private Button btnVerify, btnPaste;
    private Label lbl;
    private int attempts = 0;
    private const int MaxAttempts = 8;

    // Paste your public_key.xml content between the quotes:
    private const string PUBLIC_KEY_XML = @"//HERE";

    public LoginForm()
    {
        Text = "MYAPP — Access Required"; // <--What name you like
        Width = 600; Height = 200;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog; MaximizeBox = false; MinimizeBox = false;

        lbl = new Label { Left = 12, Top = 12, Width = 560, Text = "Paste the access token. Only the owner can generate it." };
        tbToken = new TextBox { Left = 12, Top = 50, Width = 460 };
        tbToken.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) VerifyAndProceed(); };

        btnVerify = new Button { Left = 480, Top = 48, Width = 90, Text = "Verify" };
        btnVerify.Click += (s, e) => VerifyAndProceed();

        btnPaste = new Button { Left = 480, Top = 85, Width = 90, Text = "Paste" };
        btnPaste.Click += (s, e) => { if (Clipboard.ContainsText()) tbToken.Text = Clipboard.GetText(); };

        Controls.Add(lbl); Controls.Add(tbToken); Controls.Add(btnVerify); Controls.Add(btnPaste);
    }

    private void VerifyAndProceed()
    {
        var token = tbToken.Text?.Trim();
        if (string.IsNullOrEmpty(token))
        {
            MessageBox.Show("Paste a token first.", "No token", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        string hwid = GetHwId(); // return null if you don't bind
        var res = TokenVerifier.VerifyToken(token, PUBLIC_KEY_XML, expectedRole: "owner", currentHwid: hwid);
        if (res.Valid)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
        else
        {
            attempts++;
            MessageBox.Show("Access denied: " + res.Reason, "Invalid token", MessageBoxButtons.OK, MessageBoxIcon.Error);
            if (attempts >= MaxAttempts)
            {
                MessageBox.Show("Too many failed attempts. Exiting.", "Locked", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }

    // Keep simple for now; return null to disable HWID binding
    private string GetHwId() => null;
}
