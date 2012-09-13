using System;
using System.Windows.Forms;

namespace UnityDemo
{
    internal partial class AmountEntryForm : Form
    {
        private const string promptString = "Please enter the amount that you wish to {0}:";
        private decimal amount;

        public AmountEntryForm(AmountDialogType dialogType)
        {
            InitializeComponent();

            if (dialogType == AmountDialogType.Deposit)
            {
                promptLabel.Text = String.Format(promptString, "deposit");
                this.Text = "Deposit";
            }
            else if (dialogType == AmountDialogType.Withdraw)
            {
                promptLabel.Text = String.Format(promptString, "withdraw");
                this.Text = "Withdraw";
            }
        }

        public decimal Amount
        {
            get { return amount; }
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            bool success = decimal.TryParse(amountTextBox.Text, out amount);
            if (!success)
            {
                errorProvider.SetError(amountTextBox, "Please enter a valid decimal amount.");
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }
    }

    internal enum AmountDialogType
    {
        Deposit,
        Withdraw
    }

}
