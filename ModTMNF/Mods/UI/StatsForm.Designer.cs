namespace ModTMNF.Mods.UI
{
    partial class StatsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.pausePhysicsCheckBox = new System.Windows.Forms.CheckBox();
            this.timerInfoLabel = new System.Windows.Forms.Label();
            this.pauseCoreRunCheckBox = new System.Windows.Forms.CheckBox();
            this.stepCoreButton = new System.Windows.Forms.Button();
            this.relativeSpeedLabel = new System.Windows.Forms.Label();
            this.relativeSpeedNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.inputInfoLabl = new System.Windows.Forms.Label();
            this.applyRelativeSpeedBtn = new System.Windows.Forms.Button();
            this.physicsStepApply = new System.Windows.Forms.Button();
            this.physicsStepNumericUpDown = new System.Windows.Forms.NumericUpDown();
            this.physicsStepLabel = new System.Windows.Forms.Label();
            this.driveReplayCheckBox = new System.Windows.Forms.CheckBox();
            this.replayFilePathTextBox = new System.Windows.Forms.TextBox();
            this.autoFetchReplayCheckBox = new System.Windows.Forms.CheckBox();
            this.startAtTimeLabel = new System.Windows.Forms.Label();
            this.startAtTimeTextBox = new System.Windows.Forms.TextBox();
            this.useRegularInputCheckBox = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.relativeSpeedNumericUpDown)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.physicsStepNumericUpDown)).BeginInit();
            this.SuspendLayout();
            // 
            // pausePhysicsCheckBox
            // 
            this.pausePhysicsCheckBox.AutoSize = true;
            this.pausePhysicsCheckBox.Location = new System.Drawing.Point(252, 525);
            this.pausePhysicsCheckBox.Name = "pausePhysicsCheckBox";
            this.pausePhysicsCheckBox.Size = new System.Drawing.Size(92, 17);
            this.pausePhysicsCheckBox.TabIndex = 0;
            this.pausePhysicsCheckBox.Text = "PausePhysics";
            this.pausePhysicsCheckBox.UseVisualStyleBackColor = true;
            this.pausePhysicsCheckBox.CheckedChanged += new System.EventHandler(this.pausePhysicsCheckBox_CheckedChanged);
            // 
            // timerInfoLabel
            // 
            this.timerInfoLabel.AutoSize = true;
            this.timerInfoLabel.Location = new System.Drawing.Point(3, 5);
            this.timerInfoLabel.Name = "timerInfoLabel";
            this.timerInfoLabel.Size = new System.Drawing.Size(56, 13);
            this.timerInfoLabel.TabIndex = 1;
            this.timerInfoLabel.Text = "Timer info:";
            // 
            // pauseCoreRunCheckBox
            // 
            this.pauseCoreRunCheckBox.AutoSize = true;
            this.pauseCoreRunCheckBox.Location = new System.Drawing.Point(252, 502);
            this.pauseCoreRunCheckBox.Name = "pauseCoreRunCheckBox";
            this.pauseCoreRunCheckBox.Size = new System.Drawing.Size(98, 17);
            this.pauseCoreRunCheckBox.TabIndex = 2;
            this.pauseCoreRunCheckBox.Text = "PauseCoreRun";
            this.pauseCoreRunCheckBox.UseVisualStyleBackColor = true;
            this.pauseCoreRunCheckBox.CheckedChanged += new System.EventHandler(this.pauseCoreRunCheckBox_CheckedChanged);
            // 
            // stepCoreButton
            // 
            this.stepCoreButton.Location = new System.Drawing.Point(353, 498);
            this.stepCoreButton.Name = "stepCoreButton";
            this.stepCoreButton.Size = new System.Drawing.Size(75, 23);
            this.stepCoreButton.TabIndex = 3;
            this.stepCoreButton.Text = "Step core";
            this.stepCoreButton.UseVisualStyleBackColor = true;
            this.stepCoreButton.Click += new System.EventHandler(this.stepCoreButton_Click);
            // 
            // relativeSpeedLabel
            // 
            this.relativeSpeedLabel.AutoSize = true;
            this.relativeSpeedLabel.Location = new System.Drawing.Point(25, 502);
            this.relativeSpeedLabel.Name = "relativeSpeedLabel";
            this.relativeSpeedLabel.Size = new System.Drawing.Size(75, 13);
            this.relativeSpeedLabel.TabIndex = 4;
            this.relativeSpeedLabel.Text = "relativeSpeed:";
            // 
            // relativeSpeedNumericUpDown
            // 
            this.relativeSpeedNumericUpDown.DecimalPlaces = 2;
            this.relativeSpeedNumericUpDown.Location = new System.Drawing.Point(106, 500);
            this.relativeSpeedNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.relativeSpeedNumericUpDown.Name = "relativeSpeedNumericUpDown";
            this.relativeSpeedNumericUpDown.Size = new System.Drawing.Size(79, 20);
            this.relativeSpeedNumericUpDown.TabIndex = 5;
            this.relativeSpeedNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // inputInfoLabl
            // 
            this.inputInfoLabl.AutoSize = true;
            this.inputInfoLabl.Location = new System.Drawing.Point(305, 9);
            this.inputInfoLabl.Name = "inputInfoLabl";
            this.inputInfoLabl.Size = new System.Drawing.Size(54, 13);
            this.inputInfoLabl.TabIndex = 6;
            this.inputInfoLabl.Text = "Input info:";
            // 
            // applyRelativeSpeedBtn
            // 
            this.applyRelativeSpeedBtn.Location = new System.Drawing.Point(188, 499);
            this.applyRelativeSpeedBtn.Name = "applyRelativeSpeedBtn";
            this.applyRelativeSpeedBtn.Size = new System.Drawing.Size(54, 23);
            this.applyRelativeSpeedBtn.TabIndex = 7;
            this.applyRelativeSpeedBtn.Text = "Apply";
            this.applyRelativeSpeedBtn.UseVisualStyleBackColor = true;
            this.applyRelativeSpeedBtn.Click += new System.EventHandler(this.applyRelativeSpeedBtn_Click);
            // 
            // physicsStepApply
            // 
            this.physicsStepApply.Location = new System.Drawing.Point(188, 524);
            this.physicsStepApply.Name = "physicsStepApply";
            this.physicsStepApply.Size = new System.Drawing.Size(54, 23);
            this.physicsStepApply.TabIndex = 10;
            this.physicsStepApply.Text = "Apply";
            this.physicsStepApply.UseVisualStyleBackColor = true;
            this.physicsStepApply.Click += new System.EventHandler(this.physicsStepApply_Click);
            // 
            // physicsStepNumericUpDown
            // 
            this.physicsStepNumericUpDown.Location = new System.Drawing.Point(106, 525);
            this.physicsStepNumericUpDown.Maximum = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.physicsStepNumericUpDown.Name = "physicsStepNumericUpDown";
            this.physicsStepNumericUpDown.Size = new System.Drawing.Size(79, 20);
            this.physicsStepNumericUpDown.TabIndex = 9;
            this.physicsStepNumericUpDown.Value = new decimal(new int[] {
            1,
            0,
            0,
            0});
            // 
            // physicsStepLabel
            // 
            this.physicsStepLabel.AutoSize = true;
            this.physicsStepLabel.Location = new System.Drawing.Point(30, 528);
            this.physicsStepLabel.Name = "physicsStepLabel";
            this.physicsStepLabel.Size = new System.Drawing.Size(67, 13);
            this.physicsStepLabel.TabIndex = 8;
            this.physicsStepLabel.Text = "physicsStep:";
            // 
            // driveReplayCheckBox
            // 
            this.driveReplayCheckBox.AutoSize = true;
            this.driveReplayCheckBox.Checked = true;
            this.driveReplayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.driveReplayCheckBox.Location = new System.Drawing.Point(6, 449);
            this.driveReplayCheckBox.Name = "driveReplayCheckBox";
            this.driveReplayCheckBox.Size = new System.Drawing.Size(82, 17);
            this.driveReplayCheckBox.TabIndex = 11;
            this.driveReplayCheckBox.Text = "driveReplay";
            this.driveReplayCheckBox.UseVisualStyleBackColor = true;
            this.driveReplayCheckBox.CheckedChanged += new System.EventHandler(this.driveReplayCheckBox_CheckedChanged);
            // 
            // replayFilePathTextBox
            // 
            this.replayFilePathTextBox.Location = new System.Drawing.Point(78, 471);
            this.replayFilePathTextBox.Name = "replayFilePathTextBox";
            this.replayFilePathTextBox.Size = new System.Drawing.Size(651, 20);
            this.replayFilePathTextBox.TabIndex = 12;
            this.replayFilePathTextBox.TextChanged += new System.EventHandler(this.replayFilePathTextBox_TextChanged);
            // 
            // autoFetchReplayCheckBox
            // 
            this.autoFetchReplayCheckBox.AutoSize = true;
            this.autoFetchReplayCheckBox.Checked = true;
            this.autoFetchReplayCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.autoFetchReplayCheckBox.Location = new System.Drawing.Point(6, 473);
            this.autoFetchReplayCheckBox.Name = "autoFetchReplayCheckBox";
            this.autoFetchReplayCheckBox.Size = new System.Drawing.Size(74, 17);
            this.autoFetchReplayCheckBox.TabIndex = 13;
            this.autoFetchReplayCheckBox.Text = "autoFetch";
            this.autoFetchReplayCheckBox.UseVisualStyleBackColor = true;
            this.autoFetchReplayCheckBox.CheckedChanged += new System.EventHandler(this.fetchReplayCheckBox_CheckedChanged);
            // 
            // startAtTimeLabel
            // 
            this.startAtTimeLabel.AutoSize = true;
            this.startAtTimeLabel.Location = new System.Drawing.Point(206, 450);
            this.startAtTimeLabel.Name = "startAtTimeLabel";
            this.startAtTimeLabel.Size = new System.Drawing.Size(60, 13);
            this.startAtTimeLabel.TabIndex = 14;
            this.startAtTimeLabel.Text = "startAtTime";
            // 
            // startAtTimeTextBox
            // 
            this.startAtTimeTextBox.Location = new System.Drawing.Point(266, 447);
            this.startAtTimeTextBox.Name = "startAtTimeTextBox";
            this.startAtTimeTextBox.Size = new System.Drawing.Size(100, 20);
            this.startAtTimeTextBox.TabIndex = 15;
            this.startAtTimeTextBox.TextChanged += new System.EventHandler(this.startAtTimeTextBox_TextChanged);
            // 
            // useRegularInputCheckBox
            // 
            this.useRegularInputCheckBox.AutoSize = true;
            this.useRegularInputCheckBox.Location = new System.Drawing.Point(373, 449);
            this.useRegularInputCheckBox.Name = "useRegularInputCheckBox";
            this.useRegularInputCheckBox.Size = new System.Drawing.Size(128, 17);
            this.useRegularInputCheckBox.TabIndex = 16;
            this.useRegularInputCheckBox.Text = "then use regular input";
            this.useRegularInputCheckBox.UseVisualStyleBackColor = true;
            this.useRegularInputCheckBox.CheckedChanged += new System.EventHandler(this.useRegularInputCheckBox_CheckedChanged);
            // 
            // StatsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(741, 548);
            this.Controls.Add(this.useRegularInputCheckBox);
            this.Controls.Add(this.startAtTimeTextBox);
            this.Controls.Add(this.startAtTimeLabel);
            this.Controls.Add(this.autoFetchReplayCheckBox);
            this.Controls.Add(this.replayFilePathTextBox);
            this.Controls.Add(this.driveReplayCheckBox);
            this.Controls.Add(this.physicsStepApply);
            this.Controls.Add(this.physicsStepNumericUpDown);
            this.Controls.Add(this.physicsStepLabel);
            this.Controls.Add(this.applyRelativeSpeedBtn);
            this.Controls.Add(this.inputInfoLabl);
            this.Controls.Add(this.relativeSpeedNumericUpDown);
            this.Controls.Add(this.relativeSpeedLabel);
            this.Controls.Add(this.stepCoreButton);
            this.Controls.Add(this.pauseCoreRunCheckBox);
            this.Controls.Add(this.timerInfoLabel);
            this.Controls.Add(this.pausePhysicsCheckBox);
            this.Name = "StatsForm";
            this.Text = "Stats";
            ((System.ComponentModel.ISupportInitialize)(this.relativeSpeedNumericUpDown)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.physicsStepNumericUpDown)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.CheckBox pausePhysicsCheckBox;
        private System.Windows.Forms.Label timerInfoLabel;
        private System.Windows.Forms.CheckBox pauseCoreRunCheckBox;
        private System.Windows.Forms.Button stepCoreButton;
        private System.Windows.Forms.Label relativeSpeedLabel;
        private System.Windows.Forms.NumericUpDown relativeSpeedNumericUpDown;
        private System.Windows.Forms.Label inputInfoLabl;
        private System.Windows.Forms.Button applyRelativeSpeedBtn;
        private System.Windows.Forms.Button physicsStepApply;
        private System.Windows.Forms.NumericUpDown physicsStepNumericUpDown;
        private System.Windows.Forms.Label physicsStepLabel;
        private System.Windows.Forms.CheckBox driveReplayCheckBox;
        private System.Windows.Forms.TextBox replayFilePathTextBox;
        private System.Windows.Forms.CheckBox autoFetchReplayCheckBox;
        private System.Windows.Forms.Label startAtTimeLabel;
        private System.Windows.Forms.TextBox startAtTimeTextBox;
        private System.Windows.Forms.CheckBox useRegularInputCheckBox;
    }
}