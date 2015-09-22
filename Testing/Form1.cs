using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Xml.Linq;
using DataAccess.BulkLoaders;
using DataAccess.Repository;
using DataAccess.Types;



namespace Testing
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            String url = "../../../Testdata/search.xml";
            XDocument results = XDocument.Load(url);

            IEnumerable<XElement> elements = results.Root.Elements("Series");
            foreach (var source in elements)
            {
                Console.WriteLine("{0}: {1}",
                    source.Element("SeriesName").Value,
                    source.Element("seriesid").Value);
            }

        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            //Get the BackgroundWorker object that raised this event.
            System.ComponentModel.BackgroundWorker worker = (System.ComponentModel.BackgroundWorker)sender;

            string filename = @"D:\Data\Dropbox\MyDB.xls";

            //Create XLS Loader
            XLSLoader loader = new XLSLoader(filename, DataAccess.Repository.XMLRepositoryFactory.Instance, worker, e);
            loader.Load();
        }

        private void backgroundWorker1_ReportProgress(object sender, ProgressChangedEventArgs e)
        {
            progress.Text = string.Format("{0}%: {1}", e.ProgressPercentage, e.UserState);
        }

        private void backgroundWorker1_Completed(object sender, RunWorkerCompletedEventArgs e)
        {
            progress.Text = "Import completed.";

        }
        private void button2_Click(object sender, EventArgs e)
        {
            progress.Text = "Starting...";
            backgroundWorker1.RunWorkerAsync();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            backgroundWorker2.RunWorkerAsync();
        }

        private void backgroundWorker2_DoWork(object sender, DoWorkEventArgs e)
        {
            string _id = textBox1.Text;
            System.ComponentModel.BackgroundWorker worker = (System.ComponentModel.BackgroundWorker)sender;

            if (_id != null && !_id.Equals(""))
            {
                TVDBLoader loader = new TVDBLoader(DataAccess.Repository.XMLRepositoryFactory.Instance, worker, e);
                loader.LoadShow(_id);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            XMLRepositoryFactory factory = XMLRepositoryFactory.Instance as XMLRepositoryFactory;
            //List all the contents
            foreach (Show _show in factory.ShowRepository.GetAll())
            {
                Console.WriteLine("{0}:", _show.Title);
                foreach (Season _season in _show.Seasons)
                {
                    Console.WriteLine("  Season {0}:", _season.SeasonNo);
                    foreach (Episode _episode in _season.Episodes)
                    {
                        Console.WriteLine("    S{0}E{1} - {2}: {3}", _season.SeasonNo, _episode.EpisodeNo, _episode.Title, _episode.Rating);
                    }
                }
            }
        }


    }

}
