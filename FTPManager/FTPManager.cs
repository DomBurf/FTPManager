using System;
using System.IO;
using System.Net;
using System.Text;

namespace FTPManager
{
    public class FTPManager
    {
        public string FTPServerAddress;
        public string FTPUser;
        public string FTPPassword;

        public FTPManager(string ftpServerAddress, string ftpUser, string ftpPassword)
        {
            FTPServerAddress = ftpServerAddress;
            FTPUser = ftpUser;
            FTPPassword = ftpPassword;
        }
        
        /// <summary>
        /// Upload a file from the local filesystem to a remote server
        /// </summary>
        /// <param name="source">source filename on the local filesystem</param>
        /// <param name="target">destination on the remote server</param>
        /// <param name="transfertype">FTP transfer type. Valid values are ASCII or BINARY</param>
        /// <returns></returns>
        public bool Upload(string source, string target, string transfertype)
        {
            bool success = false;
            const int bufferLength = 2048;
            byte[] buff = new byte[bufferLength];

            if (OkToProceed())
            {
                FileInfo fi = new FileInfo(source);
                //need to cast from System.Net.WebRequest to System.Net.FtpWebRequest
                FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + FTPServerAddress + "/" + target));
                ftpReq.Credentials = new NetworkCredential(FTPUser, FTPPassword);
                ftpReq.KeepAlive = false;
                //specify the command to be executed
                ftpReq.Method = WebRequestMethods.Ftp.UploadFile;
                ftpReq.UseBinary = transfertype.ToLower() != "ascii";
                //notify the FTP server about the size of the file to be transferred
                ftpReq.ContentLength = fi.Length;

                FileStream fs = fi.OpenRead();
                Stream st = ftpReq.GetRequestStream();

                int contentLen = fs.Read(buff, 0, bufferLength);

                while (contentLen != 0)
                {
                    st.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, bufferLength);
                }

                st.Close();
                fs.Close();

                success = true;
            }
            return success;
        }

        /// <summary>
        /// Download a file from the remote FTP server to the local filesystem
        /// </summary>
        /// <param name="source">source filename on the remote server</param>
        /// <param name="target">destination on the  local filesystem</param>
        /// <param name="transfertype">FTP transfer type. Valid values are ASCII or BINARY</param>
        /// <returns></returns>
        public bool Download(string source, string target, string transfertype)
        {
            bool success = false;
            const int buffersize = 2048;
            byte[] buffer = new byte[buffersize];

            if (OkToProceed())
            {
                if (File.Exists(target))
                {
                    File.Delete(target);
                }

                FileStream outputstream = new FileStream(target, FileMode.Create);
                //need to cast from System.Net.WebRequest to System.Net.FtpWebRequest
                FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + FTPServerAddress + "/" + source));
                ftpReq.Method = WebRequestMethods.Ftp.DownloadFile;
                ftpReq.UseBinary = transfertype.ToLower() != "ascii";
                ftpReq.Credentials = new NetworkCredential(FTPUser, FTPPassword);
                //need to cast from System.Net.WebResponse to System.Net.FtpWebResponse
                FtpWebResponse response = (FtpWebResponse)ftpReq.GetResponse();
                Stream ftpStream = response.GetResponseStream();

                if (ftpStream != null)
                {
                    int readcount = ftpStream.Read(buffer, 0, buffersize);

                    while (readcount > 0)
                    {
                        outputstream.Write(buffer, 0, readcount);
                        readcount = ftpStream.Read(buffer, 0, buffersize);
                    }
                    ftpStream.Close();
                }

                outputstream.Close();
                response.Close();

                success = true;
            }
            return success;
        }

        /// <summary>
        /// delete a file from the remote FTP server
        /// </summary>
        /// <param name="filename">name of the fie on the remote FTP server</param>
        /// <returns>true if successful, otherwise false</returns>
        public bool Deletefile(string filename)
        {
            bool success = false;

            if (OkToProceed())
            {
                FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + FTPServerAddress + "/" + filename));
                ftpReq.Credentials = new NetworkCredential(FTPUser, FTPPassword);
                ftpReq.KeepAlive = false;
                ftpReq.Method = WebRequestMethods.Ftp.DeleteFile;
                FtpWebResponse ftpResp = (FtpWebResponse)ftpReq.GetResponse();
                Stream datastream = ftpResp.GetResponseStream();
                if (datastream != null)
                {
                    StreamReader sr = new StreamReader(datastream);
                    sr.ReadToEnd();
                    sr.Close();
                    datastream.Close();
                }
                
                ftpResp.Close();
                success = true;
            }
            return success;
        }

        /// <summary>
        /// Returns a list of files from a specified flder on the remote FTP server
        /// </summary>
        /// <param name="folder">Folder name on the remote FTP server</param>
        /// <param name="transfertype">FTP transfer type. Valid values are ASCII or BINARY</param>
        /// <returns>true if successful, otherwise false</returns>
        public string[] Getfilelist(string folder, string transfertype)
        {
            StringBuilder result = new StringBuilder();
            string[] filelist = null;

            if (OkToProceed())
            {
                FtpWebRequest ftpReq = (FtpWebRequest)WebRequest.Create(new Uri("ftp://" + FTPServerAddress + "/" + folder));
                ftpReq.UseBinary = transfertype.ToLower() != "ascii";
                ftpReq.Credentials = new NetworkCredential(FTPUser, FTPPassword);
                ftpReq.Method = WebRequestMethods.Ftp.ListDirectory;
                
                using (WebResponse response = ftpReq.GetResponse())
                {
                    using (StreamReader reader = new StreamReader(response.GetResponseStream()))
                    {
                        string line = reader.ReadLine();
                        while (line != null)
                        {
                            result.Append(line);
                            result.Append("\n");
                            line = reader.ReadLine();
                        }
                        result.Remove(result.ToString().LastIndexOf('\n'), 1);
                        filelist = result.ToString().Split('\n');
                        reader.Close();
                        response.Close();
                    }
                }
            }

            return filelist;
        }
        
        /// <summary>
        /// Used internally to determine if all necessary FTP information has been set
        /// </summary>
        /// <returns>true if successful, otherwise false</returns>
        private bool OkToProceed()
        {
            return (!string.IsNullOrEmpty(FTPUser) && !string.IsNullOrEmpty(FTPPassword) && !string.IsNullOrEmpty(FTPServerAddress));
        }
    }
}
