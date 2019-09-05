using EO.WebBrowser;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Footbal_View
{
    public partial class Form1 : Form
    {
        public delegate void BinaryOp(object sender, EventArgs e);
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // TODO: данная строка кода позволяет загрузить данные в таблицу "save_resDataSet.Table". При необходимости она может быть перемещена или удалена.
            try
            {
                this.tableTableAdapter.Fill(this.save_resDataSet.Table);
                UpdateView1(sender, null);
                UpdateView2(sender, null);
            }
            catch (Exception t)
            {
                MessageBox.Show(t.Message, t.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

        }

        private async void Loading(object sender, EventArgs e)
        {

            await Task.Factory.StartNew(
                                             () => Load_URL(),
                                             TaskCreationOptions.LongRunning);
            Update_foot_Click(null, null);
        }

        private void Load_URL()
        {
            try
            {
                button_loading.Invoke((MethodInvoker)delegate { button_loading.Enabled = false; });
                button1.Invoke((MethodInvoker)delegate { button1.Enabled = false; });
                //Create a ThreadRunner object
                ThreadRunner threadRunner = new ThreadRunner();

                //Create a WebView through the ThreadRunner
                WebView webView = threadRunner.CreateWebView();
                threadRunner.Send(() =>
                {
                    webView.LoadUrlAndWait("https://www.myscore.ru/");
                    webView.Capture();
                });
                webView.EvalScript("" + Properties.Resources.api.Clone());
                System.Threading.Thread.Sleep(300);
                for (int i = 0; i < (int)webView.EvalScript("$('div[class *=\"tabs__text tabs__text--default\"]').length;"); i++)
                {
                    if ((string)webView.EvalScript("$('div[class *=\"tabs__text tabs__text--default\"]')[" + i + "].innerText;") == "Коэффициенты")
                    {
                        webView.EvalScript("$('div[class *=\"tabs__text tabs__text--default\"]')[" + i + "].click();");
                        System.Threading.Thread.Sleep(300);

                        if (checkBox1.Checked && load_date.Value != DateTime.Today)
                        {
                            TimeSpan timeSpan = load_date.Value - DateTime.Today;
                            if (timeSpan.Days > 7 || timeSpan.Days < -7)
                            {
                                MessageBox.Show("На сайте нет ифнормации за это число!", "Неверная дата", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                button_loading.Invoke((MethodInvoker)delegate { button_loading.Enabled = true; });
                                button1.Invoke((MethodInvoker)delegate { button1.Enabled = true; });
                                return;
                            }
                            webView.EvalScript("$('div[class *=\"calendar__datepicker\"]').click();");
                            System.Threading.Thread.Sleep(300);
                            for (int k = 0; k < 15; k++)
                            {
                                if (((string)webView.EvalScript("$('div[class *=\"calendar__datepicker--dates\"] > div.day')[" + k + "].innerText")).Contains(load_date.Value.ToString("dd") + "/" + load_date.Value.ToString("MM")))
                                {
                                    webView.EvalScript("$('div[class *=\"calendar__datepicker--dates\"] > div.day')[" + k + "].click();");
                                    System.Threading.Thread.Sleep(300);
                                    break;
                                }
                            }
                        }

                        webView.EvalScript("$('div[class *=\"event__expander icon--expander expand\"]').click();");
                        System.Threading.Thread.Sleep(100);
                        int leght_j = (int)webView.EvalScript("$('div[class *=\"event__match event__match\"]').length");
                        for (int j = 0; j < leght_j; j++)
                        {
                            System.Data.DataRow data = save_resDataSet.Table.NewRow();

                            data["Название"] = (string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[2].innerText") + " - " + (string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[3].innerText");

                            if (checkBox1.Checked)
                            {
                                data["Дата"] = load_date.Value;
                            }
                            else
                            {
                                data["Дата"] = DateTime.Today;
                            }

                            string temp_str = "NULL";
                            if ((string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[4].innerText") == "-")
                            {
                                temp_str = " - ";
                            }
                            else
                            {
                                int temp_1 = (int)webView.EvalScript("parseInt($('div[class *=\"event__match event__match\"]')[" + j + "].children[4].children[0].innerText)");
                                int temp_2 = (int)webView.EvalScript("parseInt($('div[class *=\"event__match event__match\"]')[" + j + "].children[4].children[1].innerText)");
                                if (temp_1 == temp_2)
                                {
                                    temp_str = "Ничья";
                                }
                                else if (temp_2 > temp_1)
                                {
                                    temp_str = "Победила вторая команда";
                                }
                                else
                                {
                                    temp_str = "Победила первая команда";
                                }
                            }
                            data["Результат"] = temp_str;

                            string temp1_1;
                            string temp1_2;
                            string temp1_3;

                            if ((string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[5].innerText") == "-")
                            {
                                temp1_1 = " - ";
                            }
                            else
                            {
                                temp1_1 = (string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[5].children[0].getAttribute('alt')");
                                if (temp1_1.LastIndexOf('[') != -1)
                                {
                                    temp1_1 = temp1_1.Substring(0, temp1_1.LastIndexOf('['));
                                }
                                else if (temp1_1 == "Букмекер больше не принимает ставку.")
                                {
                                    temp1_1 = (string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[5].children[0].innerText");
                                }
                            }

                            if ((string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[6].innerText") == "-")
                            {
                                temp1_2 = " - ";
                            }
                            else
                            {
                                temp1_2 = (string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[6].children[0].getAttribute('alt')");
                                if (temp1_2.LastIndexOf('[') != -1)
                                {
                                    temp1_2 = temp1_2.Substring(0, temp1_2.LastIndexOf('['));
                                }
                                else if (temp1_2 == "Букмекер больше не принимает ставку.")
                                {
                                    temp1_2 = (string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[6].children[0].innerText");
                                }
                            }

                            if ((string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[7].innerText") == "-")
                            {
                                temp1_3 = " - ";
                            }
                            else
                            {
                                temp1_3 = (string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[7].children[0].getAttribute('alt')");
                                if (temp1_3.LastIndexOf('[') != -1)
                                {
                                    temp1_3 = temp1_3.Substring(0, temp1_3.LastIndexOf('['));
                                }
                                else if (temp1_3 == "Букмекер больше не принимает ставку.")
                                {
                                    temp1_3 = (string)webView.EvalScript("$('div[class *=\"event__match event__match\"]')[" + j + "].children[7].children[0].innerText");
                                }
                            }

                            data["Коэффициент"] = temp1_1 + " : " + temp1_2 + " : " + temp1_3;

                            if (temp_str == " - " && temp1_1 == " - " && temp1_2 == " - " && temp1_3 == " - ")
                            {
                                continue;
                            }

                            if (save_resDataSet.Table.Select("Результат = '" + data["Результат"] + "' AND Название = '" + data["Название"] + "' AND Коэффициент = '" + data["Коэффициент"] + "'").Length <= 0)
                            {
                                save_resDataSet.Table.Rows.Add(data);
                            }
                        }
                        button_loading.Invoke((MethodInvoker)delegate { button_loading.Enabled = true; });
                        button1.Invoke((MethodInvoker)delegate { button1.Enabled = true; });
                        break;
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, e.Source, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }

        }

        private void Update_foot_Click(object sender, EventArgs e)
        {
            tableBindingSource.ResetBindings(false);
            UpdateView1(null, null);
            UpdateView2(null, null);
        }

        private void TableBindingNavigatorSaveItem_Click_1(object sender, EventArgs e)
        {
            this.Validate();
            this.tableBindingSource.EndEdit();
            this.tableAdapterManager.UpdateAll(this.save_resDataSet);

        }

        private void UpdateView1(object sender, LayoutEventArgs e)
        {
            TableBindingNavigatorSaveItem_Click_1(null, null);
            listView1.Items.Clear();
            DataRow[] data = save_resDataSet.Table.Select();
            for (int i = 0; i < data.Length; i++)
            {
                if ((DateTime)data[i].ItemArray[3] == find_date_old.Value)
                {
                    ListViewItem listView = listView1.Items.Add((string)data[i].ItemArray[2]);
                    listView.SubItems.Add((string)data[i].ItemArray[1]);
                    listView.SubItems.Add((string)data[i].ItemArray[4]);
                }
            }
        }

        private void UpdateView2(object sender, LayoutEventArgs e)
        {
            TableBindingNavigatorSaveItem_Click_1(null, null);
            bool find;
            if (e == null && sender == null)
            {
                find = true;
            }
            else
            {
                find = false;
            }

            listView2.Items.Clear();
            DataRow[] data = save_resDataSet.Table.Select();
            for (int i = 0; i < data.Length; i++)
            {
                if ((DateTime)data[i].ItemArray[3] == DateTime.Today)
                {
                    if (!find || ((string)data[i].ItemArray[4]).Contains(text_find.Text))
                    {
                        ListViewItem listView = listView2.Items.Add((string)data[i].ItemArray[2]);
                        listView.SubItems.Add((string)data[i].ItemArray[1]);
                        listView.SubItems.Add((string)data[i].ItemArray[4]);
                    }
                }
            }

        }

        private void CheckBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBox1.Checked)
            {
                load_date.Enabled = true;
                load_date.Visible = true;
            }
            else
            {
                load_date.Enabled = false;
                load_date.Visible = false;
            }
        }

        private void Find_date_old_ValueChanged(object sender, EventArgs e)
        {
            UpdateView1(null, null);
        }

        private void Find_new_Click(object sender, EventArgs e)
        {
            UpdateView2(null, null);
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            UpdateView1(sender, null);
            UpdateView2(sender, null);
        }
    }
}
