using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ExifLib;
using System.Diagnostics;

namespace picloc
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { FileName = textBox1.Text, Filter = "JPEG Images (*.jpg)|*.jpg" };

            if (dlg.ShowDialog() == DialogResult.OK)
            {
                textBox1.Text = dlg.FileName;
                double lat, lng;
                if (extractData(textBox1.Text, out lat, out lng) == true)
                {
                    browseUrl(lat, lng);
                }
                else
                {
                    textBox2.Text = "Failed to decode GPS Info from picture";
                }
            }
        }

        private void browseUrl(double lat, double lng)
        {
            textBox2.Text = String.Format("http://maps.google.com/maps?q={0},{1}", lat, lng);
            try
            {
                ProcessStartInfo ps = new ProcessStartInfo(textBox2.Text);
                ps.UseShellExecute = true;
                Process p = Process.Start(ps);
            }
            catch (Exception ex)
            {
                MessageBox.Show("exception : " +  ex.Message);
            }
        }

        private bool extractData(string fileName, out double lat, out double lng)
        {
            bool retVal = false;
            ExifReader reader = null;
            lat = lng = 0;
            try
            {
                reader = new ExifReader(fileName);



                // To read a single field, use code like this:
                /*
                DateTime datePictureTaken;
                if (reader.GetTagValue<DateTime>(ExifTags.DateTimeDigitized, out datePictureTaken))
                {
                    MessageBox.Show(this, string.Format("The picture was taken on {0}", datePictureTaken), "Image information", MessageBoxButtons.OK);
                }
                */
                double[] vals;
                if (reader.GetTagValue<double[]>(ExifTags.GPSLatitude, out vals) == false) return retVal;
                string stringval;
                if (reader.GetTagValue<string>(ExifTags.GPSLatitudeRef, out stringval) == false) return retVal;
                double loc;
                loc = decodeLocation(vals);
                if (stringval == "S") loc *= -1;
                lat = loc;

                if (reader.GetTagValue<double[]>(ExifTags.GPSLongitude, out vals) == false) return retVal;
                if (reader.GetTagValue<string>(ExifTags.GPSLongitudeRef, out stringval) == false) return retVal;
                loc = decodeLocation(vals);
                if (stringval == "W") loc *= -1;
                lng = loc;
                retVal = true;
                // Parse through all available fields and generate key-value labels
                //var props = Enum.GetValues(typeof (ExifTags)).Cast<ushort>().Select(tagID =>
                //{
                //    object val;
                //    if (reader.GetTagValue(tagID, out val))
                //        return string.Format("{0}: {1}", Enum.GetName(typeof (ExifTags), tagID), RenderTag(val));

                //    return null;

                //}).Where(x => x != null).ToArray();


            }
            catch (Exception )
            {
                // Something didn't work!
                //MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                if (reader != null)
                    reader.Dispose();
            }
            return retVal;
        }

        private double decodeLocation(double[] vals)
        {
            double retVal = 0;
            if (vals.Length > 0) retVal = vals[0];
            if (vals.Length > 1) retVal += vals[1] / 60;
            if (vals.Length > 2) retVal += vals[2] / 3660;
            return retVal;
        }
    }
}

