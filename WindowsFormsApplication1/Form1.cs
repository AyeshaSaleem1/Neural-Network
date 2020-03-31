using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IronXL;

namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            neural_network();
        }

        double[] input = new double[6];
        double[] ini_weight = new double[8]; ///{0.2,0.4,-0.5,0.3,0.1,0.2,-0.3,-0.2};
        double[] ini_bias = new double[3];// {-0.4,0.2,0.1};
        double[] _weight = new double[8];
        double[] _bias = new double[3];
        double[] output = new double[6];
        
        int c = 0;
        double target_output, threshold = 0.0002/* 0.001 for more perfection*/, c_error, learning_rate = 0.5, t_threshold = 0.1;
        private void neural_network()
        {
            WorkBook workbook = WorkBook.Load("HW.csv");
            WorkSheet sheet = workbook.WorkSheets.First();
            DataTable dataTable = sheet.ToDataTable(true);
            Random r = new Random();
            for (int i = 0; i < 8; i++)
            {
                ini_weight[i] = r.NextDouble() * (0.5 - (-0.5)) + (-0.5);
            }
            for (int i = 0; i < 3; i++)
            {
                ini_bias[i] = r.NextDouble() * (1 - (-1)) + (-1);
            }
            while (true)//epoch
            {
                int j = 2;
                double[] upd_weight = new double[8];
                double[] upd_bias = new double[3];
              
                foreach (DataRow row in dataTable.Rows)
                {
                   // MessageBox.Show(row[0]+"");
                    for (int i = 0; i < 3; i++)
                    {
                        input[i] = Convert.ToDouble(row[i]);
                    }
                    target_output = Convert.ToDouble(row[3]);
                    feed_foward(ini_weight, ini_bias);
                    sheet["E"+j].Value = output[5];
                    // for c_error
                    c_error += Math.Abs( output_error);
                        j++;
                    for (int i = 0; i < 8; i++)
                    {
                        upd_weight[i] += _weight[i];
                    }
                    for (int i = 0; i < 3; i++)
                    {
                        upd_bias[i] += _bias[i];
                    }
                }
                //average biases and weight after completing one epoch
                for (int i = 0; i < 8; i++)
                {
                    upd_weight[i] = upd_weight[i]/dataTable.Rows.Count;
                }
                for (int i = 0; i < 3; i++)
                {

                    upd_bias[i] = upd_bias[i]/dataTable.Rows.Count;
                }
                c_error = c_error / dataTable.Rows.Count;

                if(c_error > t_threshold)
                {
                    ini_weight = upd_weight;
                    ini_bias = upd_bias;
                    c++;
                }
                else 
                {
                   // MessageBox.Show(c_error + " " + c);
                    workbook.SaveAs(@"C:\Users\Pc Mart\Desktop\NewExcelFile.xlsx");
                    WorkBook wb = WorkBook.Load(@"C:\Users\Pc Mart\Desktop\NewExcelFile.xlsx");
                    WorkSheet _sheet = wb.WorkSheets.First();
                    _sheet.CreateChart(IronXL.Drawing.Charts.ChartType.Scatter, 2, "D", 10, "E");
                    dataTable = _sheet.ToDataTable(true);
                    dataGridView1.DataSource = dataTable;
                    j = 2;

                    var chart = chart1.ChartAreas[0];
                    chart.AxisX.IntervalType = System.Windows.Forms.DataVisualization.Charting.DateTimeIntervalType.Number;
                    chart.AxisX.LabelStyle.Format = "";
                    chart.AxisY.LabelStyle.Format = "";
                    chart.AxisX.LabelStyle.IsEndLabelVisible = true;
                    chart.AxisY.Minimum = 0.4;
                    chart.AxisY.Interval = 0.1;
                    chart.AxisX.Enabled = System.Windows.Forms.DataVisualization.Charting.AxisEnabled.False;
                    chart1.Series[0].IsVisibleInLegend = false;
                    chart1.Series.Add("Target Output");
                    chart1.Series["Target Output"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                    chart1.Series["Target Output"].Color = Color.SeaGreen;

                    chart.BackColor = Color.Black;
                    chart1.Series.Add("Calculated Output");
                    chart1.Series["Calculated Output"].ChartType = System.Windows.Forms.DataVisualization.Charting.SeriesChartType.FastLine;
                    chart1.Series["Calculated Output"].Color = Color.Red;

                    foreach (DataRow row in dataTable.Rows)
                    {

                        chart1.Series["Target Output"].Points.AddY(sheet["D" + j].Value);
                        chart1.Series["Calculated Output"].Points.AddY(sheet["E" + j].Value);
                        j++;
                    }

                   
                    break;
                }
            }
         
        }

            double output_error = 0;
        public void feed_foward(double[] updated_weight, double[] updated_bias )
        {
            output_error = 0;
            int count = 0, wcount = 0, k = 3, bcount = 0;
            double[] hidden_error = new double[2];
            for (int i = 0; i < 3; i++)
            {
                output[i] = input[i];
            }
            //for hidden layers
            for (int i = 0; i < 2; i++)
            {
                input[k] = input[0] * updated_weight[wcount++] + input[1] * updated_weight[wcount++] + input[2] * updated_weight[wcount++] + updated_bias[bcount++];
                output[k] = 1 / (1 + Math.Pow( Math.E ,(-input[k])));
                k++;
            }
            //for output layer
            input[k] = output[3] * updated_weight[wcount++] + output[4] * updated_weight[wcount] + updated_bias[bcount];
            output[k] = 1 / (1 + Math.Pow(Math.E, (-input[k])));


            //error at output layer
            output_error = output[k] * (1 - output[k]) * (target_output - output[k]);

            if (Math.Abs( output_error) > threshold)
            {
                //hidden layer error
                for (int i = 0; i < 2; i++)
                {
                    k--;
                    hidden_error[i] = output[k] * (1 - output[k]) * (output_error * updated_weight[wcount--]);
                }
                backward_propogation(hidden_error, output_error,updated_weight,updated_bias);
            }
            else
            {
            _weight = updated_weight;
            _bias = updated_bias;
            }
        }

        private void backward_propogation(double[] hidden_error, double output_error,double[] up_weight,double[] up_bias)
        {
            double[] updated_weight = new double[8];
            double[] updated_bias = new double[3];
            int err_count = 1, bcount = 1, wcount = 0;
            ///update weight
            for (int i = 0; i < 2; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    updated_weight[wcount] = learning_rate * hidden_error[err_count] * output[j];
                    updated_weight[wcount] = up_weight[wcount] + updated_weight[wcount];
                    wcount++;
                }
                err_count--;
            }

            for (int i = 3; i < 5; i++)
            {
                updated_weight[wcount] = learning_rate * output_error * output[i];
                updated_weight[wcount] = up_weight[wcount] + updated_weight[wcount];
                wcount++;
            }

            //update biases
            for (int i = 0; i < 3; i++)
            {
                if (i == 2)
                {
                    updated_bias[i] = learning_rate *output_error;
                }
                else
                {
                    updated_bias[i] = learning_rate * hidden_error[bcount--];
                }
                updated_bias[i] = up_bias[i] + updated_bias[i];
            }

            feed_foward(updated_weight, updated_bias);
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

       
    }
}
