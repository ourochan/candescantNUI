﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using CCT.NUI.Visual;
using CCT.NUI.Core;
using CCT.NUI.Core.Clustering;
using CCT.NUI.HandTracking;
using CCT.NUI.Core.OpenNI;
using CCT.NUI.Core.Shape;
using CCT.NUI.Samples.ImageManipulation;
using CCT.NUI.KinectSDK;
using CCT.NUI.Core.Video;

// Recognition
using CCT.NUI.Recognition;
using CCT.NUI.HandTracking.TrajectoryCollector;

namespace CCT.NUI.Samples
{
    public partial class MainForm : Form
    {
        private IList<IDataSource> activeDataSources;
        private IDataSourceFactory dataSourceFactory;
        private IRecognizerDataSource recognizer;
        private ITrajectoryCollector trajectoryCollector;
        private ClusterDataSourceSettings clusteringSettings = new ClusterDataSourceSettings();
        private ShapeDataSourceSettings shapeSettings = new ShapeDataSourceSettings();
        private HandDataSourceSettings handDetectionSettings = new HandDataSourceSettings();

        public MainForm()
        {
            InitializeComponent();
            this.activeDataSources = new List<IDataSource>();
            this.FormClosing += new FormClosingEventHandler(MainForm_FormClosing);

            this.recognizer = null;
        }

        private void buttonRGB_Click(object sender, EventArgs e)
        {
            this.SetImageDataSource(this.dataSourceFactory.CreateRGBBitmapDataSource());
        }

        private void buttonDepth_Click(object sender, EventArgs e)
        {
            this.SetImageDataSource(this.dataSourceFactory.CreateDepthBitmapDataSource());
        }

        private void buttonCollectTrajectory_Click(object sender, EventArgs e)
        {
            var trajectoryDataSource = new TrajectoryDataSource
                    (new HandDataSource
                        (this.dataSourceFactory.CreateShapeDataSource(
                        this.clusteringSettings, this.shapeSettings),
                        this.handDetectionSettings));

            trajectoryDataSource.NewTrajectoryAvailable += this.trajectoryCollector.Collecct;

            this.SetTrajectoryDataSource(trajectoryDataSource);
        }
        private void SetTrajectoryDataSource(ITrajectoryDataSource dataSource)
        {
            this.SetDataSource(dataSource, new TrajectoryLayer(dataSource));
        }

        private void buttonClustering_Click(object sender, EventArgs e)
        {
            this.SetClusterDataSource(this.dataSourceFactory.CreateClusterDataSource(this.clusteringSettings));
        }

        private void SetClusterDataSource(IClusterDataSource dataSource)
        {
            this.SetDataSource(dataSource, new ClusterLayer(dataSource));
        }

        private void SetHandDataSource(IHandDataSource dataSource)
        {
            this.SetDataSource(dataSource, new HandLayer(dataSource));
        }

        private void SetDataSource(IDataSource dataSource, ILayer layer)
        {
            this.Clear();
            this.activeDataSources.Add(dataSource);
            this.videoControl.AddLayer(layer);
            dataSource.Start();
        }

        private void SetImageDataSource(IBitmapDataSource dataSource)
        {
            this.Clear();
            this.activeDataSources.Add(dataSource);
            this.videoControl.SetImageSource(dataSource);
            dataSource.Start();
        }

        private void Clear()
        {
            foreach (var dataSource in this.activeDataSources)
            {
                dataSource.Stop();
            }
            this.activeDataSources.Clear();
            this.videoControl.Clear();
            
        }

        void layer_RequestRefresh(object sender, EventArgs e)
        {
            this.videoControl.Invalidate();
        }

        void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            //this.trajectoryCollector.Serialize();

            this.Clear();
            if (this.dataSourceFactory != null)
            {
                this.dataSourceFactory.Dispose();
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void linkLabelSource_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://candescentnui.codeplex.com/");
        }

        private void linkLabelBlog_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("http://blog.candescent.ch");
        }

        private void buttonHandAndFinger_Click(object sender, EventArgs e)
        {
            this.SetHandDataSource(new HandDataSource(this.dataSourceFactory.CreateShapeDataSource(this.clusteringSettings, this.shapeSettings), this.handDetectionSettings));
        }

        private void buttonSettings_Click(object sender, EventArgs e)
        {
            new SettingsForm(this.clusteringSettings, this.shapeSettings, this.handDetectionSettings).Show();
        }

        private void radioButtonSDK_CheckedChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                this.dataSourceFactory = new SDKDataSourceFactory();
                //this.trajectoryCollector = new MockTrajectoryCollector("MockTrajectory.xml");
            }
            catch (Exception exc)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.ToggleButtons();
            Cursor.Current = Cursors.Default;
        }

        private void ToggleButtons()
        {
            this.Enable(this.buttonClustering, this.buttonDepth, this.buttonRGB, this.buttonHandAndFinger, this.buttonImageManipulation, this.buttonTrajectory, this.buttonTraining);
            this.Disable(this.radioButtonOpenNI, this.radioButtonSDK, this.radioOpenNINite, this.radioButtonKinectWONear);
        }

        private void Enable(params Control[] controls)
        {
            this.SetEnabled(controls, true);
        }

        private void Disable(params Control[] controls)
        {
            this.SetEnabled(controls, false);
        }

        private void SetEnabled(IEnumerable<Control> controls, bool value)
        {
            foreach (var control in controls)
            {
                control.Enabled = value;
            }
        }

        private void radioButtonOpenNI_CheckedChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            this.InitializeOpenNI();
            this.ToggleButtons();
            this.buttonTrajectory.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private void InitializeOpenNI()
        {
            try
            {
                this.dataSourceFactory = new OpenNIDataSourceFactory("Config.xml");
            }
            catch (Exception exc)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        private void buttonImageManipulation_Click(object sender, EventArgs e)
        {
            var dataSource = new HandDataSource(this.dataSourceFactory.CreateShapeDataSource(this.clusteringSettings, this.shapeSettings));
            new ImageForm(dataSource).Show();
            dataSource.Start();
        }

        private void buttonHandTracking_Click(object sender, EventArgs e)
        {
            this.SetImageDataSource(this.dataSourceFactory.CreateDepthBitmapDataSource());
            var dataSource = (this.dataSourceFactory as OpenNIDataSourceFactory).CreateTrackingClusterDataSource();
            var handDataSource = new HandDataSource(this.dataSourceFactory.CreateShapeDataSource(dataSource, this.shapeSettings), this.handDetectionSettings);
            this.activeDataSources.Add(handDataSource);
            var layer = new HandLayer(handDataSource);
            this.videoControl.AddLayer(layer);
            handDataSource.Start();
        }

        private void radioOpenNINite_CheckedChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            this.Disable(this.radioButtonOpenNI, this.radioButtonSDK, this.radioButtonOpenNI);
            this.InitializeOpenNI();
            //this.buttonHandTracking.Enabled = true;
            this.buttonTrajectory.Enabled = true;
            Cursor.Current = Cursors.Default;
        }

        private void radioButtonKinectWONear_CheckedChanged(object sender, EventArgs e)
        {
            Cursor.Current = Cursors.WaitCursor;
            try
            {
                this.clusteringSettings.MaximumDepthThreshold = 1000;
                this.dataSourceFactory = new SDKDataSourceFactory(useNearMode: false);
            }
            catch (Exception exc)
            {
                Cursor.Current = Cursors.Default;
                MessageBox.Show(exc.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            this.ToggleButtons();
            Cursor.Current = Cursors.Default;
        }

        private void buttonRecognitionTraining_Click(object sender, EventArgs e)
        {
            //this.recognizer =
            //     new RecognizerDataSource(
            //         new TrajectoryDataSource(
            //             new HandDataSource(this.dataSourceFactory.CreateShapeDataSource(this.clusteringSettings, this.shapeSettings), this.handDetectionSettings)));

            var mockTrajectoryDataSource = new MockTrajectoryDataSource(this.trajectoryCollector);
            this.recognizer = new RecognizerDataSource(mockTrajectoryDataSource);
            this.recognizer.SetWorkingState(RecognizerWorkingState.Train);
            SetDataSource(this.recognizer, new RecognitionLayer(this.recognizer));

            mockTrajectoryDataSource.StartMocking();
        }
        private void buttonRecognitionTesting_Click(object sender, EventArgs e)
        {
            this.recognizer.SetWorkingState(RecognizerWorkingState.Test);
            SetDataSource(this.recognizer, new RecognitionLayer(this.recognizer));
        }
    }
}
