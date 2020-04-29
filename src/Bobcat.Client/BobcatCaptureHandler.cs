// <copyright file="BobcatCaptureHandler.cs" company="Techyian">
// Copyright (c) Ian Auty. All rights reserved.
// Licensed under the MIT License. Please see LICENSE.txt for License info.
// </copyright>

using System;
using System.Diagnostics;
using System.Text;
using Microsoft.Extensions.Logging;
using MMALSharp.Common;
using MMALSharp.Common.Utility;
using MMALSharp.Handlers;

namespace Bobcat.Client
{
    public class BobcatCaptureHandler : IVideoCaptureHandler
    {
        public Process CurrentProcess { get; }

        /// <summary>
        /// The total size of data that has been processed by this capture handler.
        /// </summary>
        protected int Processed { get; set; }

        /// <summary>
        /// Creates a new instance of <see cref="BobcatCaptureHandler"/> with the specified process arguments.
        /// </summary>
        /// <param name="argument">The <see cref="ProcessStartInfo"/> argument.</param>
        public BobcatCaptureHandler(string argument)
        {
            var processStartInfo = new ProcessStartInfo
            {
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                FileName = "ffmpeg",
                Arguments = argument
            };

            this.CurrentProcess = new Process();
            CurrentProcess.StartInfo = processStartInfo;

            Console.InputEncoding = Encoding.ASCII;

            CurrentProcess.EnableRaisingEvents = true;

            CurrentProcess.ErrorDataReceived += (object sendingProcess, DataReceivedEventArgs e) =>
            {
                if (e.Data != null)
                {
                    Console.WriteLine(e.Data);
                }
            };

            CurrentProcess.Start();

            CurrentProcess.BeginErrorReadLine();
        }

        /// <summary>
        /// Returns whether this capture handler features the split file functionality.
        /// </summary>
        /// <returns>True if can split.</returns>
        public bool CanSplit() => false;

        /// <summary>
        /// Not used.
        /// </summary>
        public void PostProcess() { }

        /// <summary>
        /// Not used.
        /// </summary>
        /// <returns>A NotImplementedException.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public string GetDirectory()
            => throw new NotImplementedException();

        /// <summary>
        /// Not used.
        /// </summary>
        /// <param name="allocSize">N/A.</param>
        /// <returns>A NotImplementedException.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public ProcessResult Process(uint allocSize)
            => throw new NotImplementedException();

        public void Process(ImageContext context)
        {
            try
            {
                CurrentProcess.StandardInput.BaseStream.Write(context.Data, 0, context.Data.Length);
                CurrentProcess.StandardInput.BaseStream.Flush();
                this.Processed += context.Data.Length;
            }
            catch
            {
                CurrentProcess.Kill();
                throw;
            }
        }

        /// <summary>
        /// Not used.
        /// </summary>
        /// <exception cref="NotImplementedException"></exception>
        public void Split()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns the total number of bytes processed by this capture handler.
        /// </summary>
        /// <returns>The total number of bytes processed by this capture handler.</returns>
        public string TotalProcessed()
        {
            return $"{this.Processed}";
        }

        /// <inheritdoc />
        public void Dispose()
        {
            MMALLog.Logger.LogInformation("Disposing capture handler.");

            if (!CurrentProcess.HasExited)
            {
                CurrentProcess.Kill();
            }
        }
    }
}
