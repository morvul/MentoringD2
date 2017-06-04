using System;
using System.Windows.Forms;

namespace Profiling
{
    public partial class MainForm : Form
    {
        private readonly MemoryLeaksGenerator _memLeakGenerator;
        private bool _isUnmanLeakInProgress;
        private bool _isManLeakInProgress;

        public MainForm()
        {
            InitializeComponent();
            _memLeakGenerator = new MemoryLeaksGenerator();
            _isUnmanLeakInProgress = false;
            _isManLeakInProgress = false;
        }

        private void UnmanagedLeakButton_Click(object sender, EventArgs e)
        {
            if (_isUnmanLeakInProgress)
            {
                _memLeakGenerator.StopGenerateUnmanagedLeak();
                UnmanagedLeakButton.Text = "Start Unmanaged leak";
            }
            else
            {
                _memLeakGenerator.StartGenerateUnmanagedLeak();
                UnmanagedLeakButton.Text = "Stop Unmanaged leak";
            }

            _isUnmanLeakInProgress = !_isUnmanLeakInProgress;
        }

        private void ManagedLeakButton_Click(object sender, EventArgs e)
        {
            if (_isManLeakInProgress)
            {
                _memLeakGenerator.StopGenerateManagedLeak();
                ManagedLeakButton.Text = "Start Managed leak";
            }
            else
            {
                _memLeakGenerator.StartGenerateManagedLeak();
                ManagedLeakButton.Text = "Stop Managed leak";
            }

            _isManLeakInProgress = !_isManLeakInProgress;
        }
    }
}
