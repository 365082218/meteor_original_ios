//
//   Copyright (c) 2014 Outfit7. All rights reserved.
//

using System;

namespace Outfit7.Util.Io {

    /// <summary>
    /// Simple file reader and writer.
    /// </summary>
    public class SimpleFileReaderWriter {

        protected const string Tag = "FileReaderWriter";
        protected readonly object Lock = new object();
        protected readonly string FilePath;

        // Getter is not thread safe, not needed to be
        public DateTime SaveTime { get; private set; }

        public SimpleFileReaderWriter(string filePath) {
            Assert.HasText(filePath, "filePath");
            this.FilePath = filePath;
        }

        public virtual string ReadContent() {
            lock (Lock) {
                SaveTime = ReadFileWriteTime();
                return ReadFile();
            }
        }

        protected virtual DateTime ReadFileWriteTime() {
            if (!O7File.Exists(FilePath))
                return DateTime.MinValue;

            DateTime writeTime = O7File.GetLastWriteTime(FilePath);

            O7Log.VerboseT(Tag, "File {0} last written time: {1}", FilePath, writeTime.ToLocalTime());

            return writeTime;
        }

        protected virtual string ReadFile() {
            if (!O7File.Exists(FilePath))
                return null;

            O7Log.DebugT(Tag, "Reading data from file {0}", FilePath);

            try {
                return O7File.ReadAllText(FilePath);

            } catch (Exception e) { // This should not happen under normal circumstances, but MTT-2836
                O7Log.WarnT(Tag, e, "Cannot read data from file {0}", FilePath);
                return null;
            }
        }

        public virtual void WriteContent(string content) {
            lock (Lock) {
                bool success = WriteFile(content);
                if (success) {
                    SaveTime = DateTime.UtcNow;
                }
            }
        }

        protected virtual bool WriteFile(string data) {
            O7Log.DebugT(Tag, "Writing data to file {0}", FilePath);

            try {
                O7File.WriteAllText(FilePath, data);
                return true;

            } catch (Exception e) { // This should not happen under normal circumstances, but MTT-2836
                O7Log.WarnT(Tag, e, "Cannot write data to file {0}", FilePath);
                return false;
            }
        }
    }
}
