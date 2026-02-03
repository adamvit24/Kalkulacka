using System;
using System.Globalization;
using System.Windows;
using System.Windows.Input;

namespace WpfApp1
{
    public partial class MainWindow : Window
    {
        private double _accumulator = 0;
        private string? _pendingOperator = null;
        private bool _isNewEntry = true;
        private bool _lastWasOperator = false;

        public MainWindow()
        {
            InitializeComponent();
            this.PreviewKeyDown += MainWindow_PreviewKeyDown;
            this.Loaded += (s, e) => this.Focus();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // Číslice (0-9) - standardní klávesy
            if ((e.Key >= Key.D0 && e.Key <= Key.D9) && (Keyboard.Modifiers & ModifierKeys.Control) == 0)
            {
                int digit = e.Key - Key.D0;
                AppendText(digit.ToString());
                e.Handled = true;
                return;
            }
            // Číslice z numpad (0-9)
            if (e.Key >= Key.NumPad0 && e.Key <= Key.NumPad9)
            {
                int digit = e.Key - Key.NumPad0;
                AppendText(digit.ToString());
                e.Handled = true;
                return;
            }
            // Tečka (desetinná čárka)
            if (e.Key == Key.OemPeriod || e.Key == Key.Decimal)
            {
                DotButton_Click(null, null);
                e.Handled = true;
                return;
            }
            // Plus (numpad +)
            if (e.Key == Key.Add || (e.Key == Key.OemPlus && (Keyboard.Modifiers & ModifierKeys.Shift) != 0))
            {
                HandleOperatorKey("+");
                e.Handled = true;
                return;
            }
            // Minus (numpad - nebo OemMinus)
            if (e.Key == Key.Subtract || e.Key == Key.OemMinus)
            {
                HandleOperatorKey("-");
                e.Handled = true;
                return;
            }
            // Krát (násobení - numpad *)
            if (e.Key == Key.Multiply)
            {
                HandleOperatorKey("*");
                e.Handled = true;
                return;
            }
            // Děleno (numpad /)
            if (e.Key == Key.Divide)
            {
                HandleOperatorKey("/");
                e.Handled = true;
                return;
            }
            // Rovná se (Enter nebo =)
            if (e.Key == Key.Return)
            {
                EqualsButton_Click(null, null);
                e.Handled = true;
                return;
            }
            // Backspace
            if (e.Key == Key.Back)
            {
                BackspaceButton_Click(null, null);
                e.Handled = true;
                return;
            }
            // Escape (Clear)
            if (e.Key == Key.Escape)
            {
                ClearButton_Click(null, null);
                e.Handled = true;
                return;
            }
        }

        private void HandleOperatorKey(string op)
        {
            var displayOp = op;
            if (op == "*") displayOp = "×";
            else if (op == "/") displayOp = "÷";
            else if (op == "-") displayOp = "−";

            var btn = new System.Windows.Controls.Button { Content = displayOp };
            OperatorButton_Click(btn, null);
        }

        private void DigitButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn)
            {
                AppendText(btn.Content?.ToString());
            }
        }

        private void DotButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isNewEntry)
            {
                Display.Text = "0";
                _isNewEntry = false;
            }

            if (!Display.Text.Contains("."))
                Display.Text += ".";
        }

        private void AppendText(string? text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            if (_isNewEntry)
            {
                Display.Text = text;
                _isNewEntry = false;
                _lastWasOperator = false;
            }
            else
            {
                Display.Text += text;
                _lastWasOperator = false;
            }
        }

        private void OperatorButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is System.Windows.Controls.Button btn)
            {
                var op = btn.Content?.ToString();
                if (op == null) return;

                // normalize operators
                if (op == "×") op = "*";
                else if (op == "÷") op = "/";
                else if (op == "−") op = "-";

                if (!double.TryParse(Display.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var currentValue))
                {
                    Display.Text = "Error";
                    return;
                }

                // Pokud byl právě stisknut operátor, jen ho vyměň
                if (_lastWasOperator)
                {
                    _pendingOperator = op;
                    return;
                }

                // Pokud je nějaký operátor čekající, spočítej výsledek
                if (_pendingOperator != null)
                {
                    Compute();
                }
                else
                {
                    _accumulator = currentValue;
                }

                _pendingOperator = op;
                _isNewEntry = true;
                _lastWasOperator = true;
            }
        }

        private void EqualsButton_Click(object sender, RoutedEventArgs e)
        {
            if (_pendingOperator != null)
            {
                Compute();
            }
            _pendingOperator = null;
            _isNewEntry = true;
            _lastWasOperator = false;
        }

        private void Compute()
        {
            if (!double.TryParse(Display.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var current))
            {
                Display.Text = "Error";
                return;
            }

            try
            {
                switch (_pendingOperator)
                {
                    case "+":
                        _accumulator = _accumulator + current;
                        break;
                    case "-":
                        _accumulator = _accumulator - current;
                        break;
                    case "*":
                        _accumulator = _accumulator * current;
                        break;
                    case "/":
                        if (current == 0)
                        {
                            Display.Text = "Cannot divide by 0";
                            _isNewEntry = true;
                            return;
                        }
                        _accumulator = _accumulator / current;
                        break;
                    case "%":
                        _accumulator = _accumulator % current;
                        break;
                    case null:
                        _accumulator = current;
                        break;
                    default:
                        // unknown operator
                        _accumulator = current;
                        break;
                }

                Display.Text = _accumulator.ToString(CultureInfo.InvariantCulture);
            }
            catch
            {
                Display.Text = "Error";
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            _accumulator = 0;
            _pendingOperator = null;
            _isNewEntry = true;
            _lastWasOperator = false;
            Display.Text = "0";
        }

        private void BackspaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (_isNewEntry)
            {
                Display.Text = "0";
                return;
            }

            if (Display.Text.Length <= 1)
            {
                Display.Text = "0";
                _isNewEntry = true;
            }
            else
            {
                Display.Text = Display.Text.Substring(0, Display.Text.Length - 1);
            }
        }

        private void PlusMinusButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Display.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            {
                v = -v;
                Display.Text = v.ToString(CultureInfo.InvariantCulture);
            }
        }

        private void ReciprocalButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Display.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            {
                if (v == 0)
                {
                    Display.Text = "Cannot divide by 0";
                    return;
                }
                v = 1 / v;
                Display.Text = v.ToString(CultureInfo.InvariantCulture);
                _isNewEntry = true;
            }
        }

        private void SquareButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Display.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            {
                v = v * v;
                Display.Text = v.ToString(CultureInfo.InvariantCulture);
                _isNewEntry = true;
            }
        }

        private void SqrtButton_Click(object sender, RoutedEventArgs e)
        {
            if (double.TryParse(Display.Text, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
            {
                if (v < 0)
                {
                    Display.Text = "Cannot sqrt negative";
                    return;
                }
                v = Math.Sqrt(v);
                Display.Text = v.ToString(CultureInfo.InvariantCulture);
                _isNewEntry = true;
            }
        }

        private void ClearEntryButton_Click(object sender, RoutedEventArgs e)
        {
            Display.Text = "0";
            _isNewEntry = true;
            _lastWasOperator = false;
        }
    }
}