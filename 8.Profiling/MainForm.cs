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
            }
            else
            {
                _memLeakGenerator.StartGenerateUnmanagedLeak();
            }

            _isUnmanLeakInProgress = !_isUnmanLeakInProgress;
        }

        private void ManagedLeakButton_Click(object sender, EventArgs e)
        {
            if (_isManLeakInProgress)
            {
                _memLeakGenerator.StopGenerateManagedLeak();
            }
            else
            {
                _memLeakGenerator.StartGenerateManagedLeak();
            }

            _isManLeakInProgress = !_isManLeakInProgress;
        }
    }
}
