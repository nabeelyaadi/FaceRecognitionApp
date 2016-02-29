using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;

namespace FaceRecognitionApp
{
    public partial class NasfiaAttendanceApp : Form
    {
        StreamWriter sw;
        StreamReader sr;
        public NasfiaAttendanceApp()
        {
            InitializeComponent();
        }
        public zkemkeeper.CZKEM zkemKeeper = new zkemkeeper.CZKEM();

        private bool bIsConnected = false;//the boolean value identifies whether the device is connected
        private int iMachineNumber = 1;//the serial number of the device.After connecting the device ,this value will be changed.

        private void btnConnect_Click(object sender, EventArgs e)
        {
            try
            {


                if (txtip.Text.Trim() == "" || txtport.Text.Trim() == "")
                {
                    MessageBox.Show("IP and Port cannot be null", "Error");
                    return;
                }
                int idwErrorCode = 0;

                Cursor = Cursors.WaitCursor;
                if (btnConnect.Text == "DisConnect")
                {
                    zkemKeeper.Disconnect();
                    bIsConnected = false;
                    btnConnect.Text = "Connect";
                    lblState.Text = "Current State:DisConnected";
                    Cursor = Cursors.Default;
                    return;
                }

                bIsConnected = zkemKeeper.Connect_Net(txtip.Text, Convert.ToInt32(txtport.Text));
                if (bIsConnected == true)
                {
                    btnConnect.Text = "DisConnect";
                    btnConnect.Refresh();
                    lblState.Text = "Current State:Connected";
                    iMachineNumber = 1;//In fact,when you are using the tcp/ip communication,this parameter will be ignored,that is any integer will all right.Here we use 1.
                    zkemKeeper.RegEvent(iMachineNumber, 65535);//Here you can register the realtime events that you want to be triggered(the parameters 65535 means registering all)
                }
                else
                {
                    zkemKeeper.GetLastError(ref idwErrorCode);
                    MessageBox.Show("Unable to connect the device,ErrorCode=" + idwErrorCode.ToString(), "Error");
                }
                Cursor = Cursors.Default;
            }
            catch (Exception)
            {

                MessageBox.Show("Error");
            }
        }



        private void btnDownloadTmp_Click(object sender, EventArgs e)
        {
            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first!", "Error");
                return;
            }

            string sdwEnrollNumber = "";
            string sName = "";
            string sPassword = "";
            int iPrivilege = 0;
            bool bEnabled = false;

            int idwFingerIndex;
            string sTmpData = "";
            int iTmpLength = 0;
            int iFlag = 0;

            lvDownload.Items.Clear();
            lvDownload.BeginUpdate();
            zkemKeeper.EnableDevice(iMachineNumber, false);
            Cursor = Cursors.WaitCursor;

            zkemKeeper.ReadAllUserID(iMachineNumber);//read all the user information to the memory
            zkemKeeper.ReadAllTemplate(iMachineNumber);//read all the users' fingerprint templates to the memory
            while (zkemKeeper.SSR_GetAllUserInfo(iMachineNumber, out sdwEnrollNumber, out sName, out sPassword, out iPrivilege, out bEnabled))//get all the users' information from the memory
            {
                for (idwFingerIndex = 0; idwFingerIndex < 10; idwFingerIndex++)
                {
                    if (zkemKeeper.GetUserTmpExStr(iMachineNumber, sdwEnrollNumber, idwFingerIndex, out iFlag, out sTmpData, out iTmpLength))//get the corresponding templates string and length from the memory
                    {
                        ListViewItem list = new ListViewItem();
                        list.Text = sdwEnrollNumber;
                        list.SubItems.Add(sName);
                        list.SubItems.Add(idwFingerIndex.ToString());
                        list.SubItems.Add(sTmpData);
                        list.SubItems.Add(iPrivilege.ToString());
                        list.SubItems.Add(sPassword);
                        //sw.WriteLine("Machine No :" + iMachineNumber);
                        //sw.WriteLine("Enrollment No :"+sdwEnrollNumber);
                        //sw.WriteLine("FingerIndex :"+idwFingerIndex.ToString());
                        //sw.WriteLine("Flag :" + iFlag);
                        //sw.WriteLine(sTmpData);
                        //sw.WriteLine("Legth :" + iTmpLength);

                        if (bEnabled == true)
                        {
                            list.SubItems.Add("true");
                        }
                        else
                        {
                            list.SubItems.Add("false");
                        }
                        list.SubItems.Add(iFlag.ToString());
                        lvDownload.Items.Add(list);
                    }
                }
            }
            //sw.Close();
            lvDownload.EndUpdate();
            zkemKeeper.EnableDevice(iMachineNumber, true);
            Cursor = Cursors.Default;
        }
        public void setuser(int iMachineNumber, string EnrollmentNo, string Name, string Password, int iPrivilege, bool bEnabled)
        {
            int idwErrorCode = 0;
            Cursor = Cursors.WaitCursor;
            if (zkemKeeper.SSR_SetUserInfo(iMachineNumber, EnrollmentNo, Name, Password, iPrivilege, bEnabled))
            {
                // zkemKeeper.RefreshData(iMachineNumber);//the data in the device should be refreshed
                MessageBox.Show("UserAdded");
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }
        public void setuserfingerprint()
        {
            int idwErrorCode = 0;

            string tmp = "SolTUzIxAAADyskECAUHCc7QAAAby2kBAAAAg3cfpMo/AIMPggCBAPzF3gBWAB8PlwBzymEP2QB3AOEP7Mp6AJ8PjABOAGzEqQCOACQPzgGNyioOkwCRANEOpMqWAI8OPgBbAFLFXACiAOEPVQCsyjcP9QCxAPMPSMrEAEUPjwAKAEPFmwDTADwPMgDVyrkPLQDZAIQP7MrgAD8PCAEjADrFPgD4AMEPyQH9yrgOYgAIAXsPxMoNAUQPkQDSAcDFxAAcAcUPUgBAy8YOawBKAQgOV8pMAZYPeoXKZyCpf33no7+Tb/2RSPKfcxFbgfbzZkwnBc8CaX4/dZFIMV4VTybgZZba2+C37o2u9i4KC2x4hWnxtG64fYXqN3BmDgPx45D1VFqBEiSO+O+oiUsjDpP4pgXXZvE8UQcCDBOzEf/5ww6vIXjpgkOFaUsr/Q/zHYiMCmFA5IKBgtH+r4HRzIp9gYJqfrMDVTTS/P/zgYByghzN+/9LBMMBRIBAzyIDdgc2/1oHLvCvC48P5coi07QRdh2oRCAvxAI31RUEALwlBu4MA7wq9P03OEbPAKDwh4vCbsEFxas9w/9MBgBLQDVE/cEBjC8GwP+DVPzCAX5Ed8DCucEJyodGA8H//f1GBcrjVRpG/wXF4FnowFAGAFBypVvHxQHVc5p7xATAk73AEQA1deA6QPw0/8H//UbAOgwDF3YeOEBT/88A2rEj/sH+wf46/8PMAfN/JP7AgwQDQol0lREAiUtwwArCwsGHfsE4wP/bAayRIP5AOktdkf4fAEGa3IL+/eMpwP0+/1r3wEPyEQCrmyT+hT1BjMEZAFye1zj+/jT8//4qRkYFwP0LNAcAOqBXTk0GykGiUMB4DcVapYXAdsL/iyzNAJJ4QcJGUw8AMrU5mcHCNf7AO84AlHExwP8xwULPAEkDR3PBwcJP3wAiB8czRMH//evA/TfA/v7//8A6OgXKj9I9VcALxZvb8P7/S3b6BcUq3oxxCgAv3T2dhMEOCwDv5D1UBT786AFA9L0wYOv/IOAxwf1GYErqBhLHA0DAMBwQpwS+nsH9//79/Tz+/TbB//7/wP4E/sIK/v/+BxCQ3kbCNf0jBhAoHP/D/GAqEE5CqTWUO8M0w/3AMf3+Ov39Cv/+wP7A/gX//QvB/sH/wP4F/B7abEfTnVVEBfv4Mvv9wTH/wDvBwwr8/wwQgEkMcVuR/g8AgrhJvnv8Cv78/yoDEcBJVAoiED5WBsQBxMEOwcL9wf/BO8D9N//5/Pv9S4X/wwrAMQQQeWPDjA==";
            Cursor = Cursors.WaitCursor;
            if (zkemKeeper.SetUserTmpExStr(1, "1", 0, 1, tmp))
            {
                // zkemKeeper.RefreshData(iMachineNumber);//the data in the device should be refreshed
                MessageBox.Show("Fingerprints Added");
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }
        public void setuserfaceprint()
        {
            
            sr = new StreamReader("face.txt");
            string tmp = sr.ReadToEnd();
            //sr.Close();
            
            //byte temp=Convert.ToByte(tmp);
            //StreamWriter sw = new StreamWriter("new.txt", true);
            
            //foreach (var item in temp)
            //{
            //    sw.Write(item);
            //}
            //sw.Close();
            sr = new StreamReader("face.txt");
            tmp = sr.ReadToEnd();
            sr.Close();
            int idwErrorCode = 0;
            Cursor = Cursors.WaitCursor;
            if (zkemKeeper.SetUserFaceStr(1, "1", 50, tmp, 41216))
            {
                // zkemKeeper.RefreshData(iMachineNumber);//the data in the device should be refreshed
                MessageBox.Show("Face Added");
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }
        static byte[] GetBytes(string str)
        {
            byte[] bytes = new byte[str.Length * sizeof(char)];
            System.Buffer.BlockCopy(str.ToCharArray(), 0, bytes, 0, bytes.Length);
            return bytes;
        }
        private void btnDownLoadFace_Click(object sender, EventArgs e)
        {
           
            sw = new StreamWriter("face.txt");

            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first!", "Error");
                return;
            }

            string sUserID = "";
            string sName = "";
            string sPassword = "";
            int iPrivilege = 0;
            bool bEnabled = false;

            int iFaceIndex = 50;//the only possible parameter value
            string sTmpData = "";
            int iLength = 0;

            lvFace.Items.Clear();
            lvFace.BeginUpdate();

            Cursor = Cursors.WaitCursor;
            zkemKeeper.EnableDevice(iMachineNumber, false);
            zkemKeeper.ReadAllUserID(iMachineNumber);//read all the user information to the memory

            while (zkemKeeper.SSR_GetAllUserInfo(iMachineNumber, out sUserID, out sName, out sPassword, out iPrivilege, out bEnabled))//get all the users' information from the memory
            {
                if (zkemKeeper.GetUserFaceStr(iMachineNumber, sUserID, iFaceIndex, ref sTmpData, ref iLength))//get the face templates from the memory
                {
                    ListViewItem list = new ListViewItem();
                    list.Text = sUserID;
                    list.SubItems.Add(sName);
                    list.SubItems.Add(sPassword);
                    list.SubItems.Add(iPrivilege.ToString());
                    list.SubItems.Add(iFaceIndex.ToString());
                    list.SubItems.Add(sTmpData);
                    list.SubItems.Add(iLength.ToString());
                    //sw.WriteLine("Machine No :" + iMachineNumber);
                    //sw.WriteLine("Enrollment No :"+sUserID);
                    //sw.WriteLine("FaceIndex :"+iFaceIndex.ToString());
                    ////sw.WriteLine("Flag :" + iFlag);
                    sw.WriteLine(sTmpData);
                    //sw.WriteLine("Legth :" + iLength);
                    if (bEnabled == true)
                    {
                        list.SubItems.Add("true");
                    }
                    else
                    {
                        list.SubItems.Add("false");
                    }
                    lvFace.Items.Add(list);
                }
            }
            sw.Close();
            zkemKeeper.EnableDevice(iMachineNumber, true);
            lvFace.EndUpdate();
            Cursor = Cursors.Default;
        }

        private void btnDelUserFace_Click(object sender, EventArgs e)
        {
            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first!", "Error");
                return;
            }

            if (cbUserID3.Text.Trim() == "")
            {
                MessageBox.Show("Please input the UserID first!", "Error");
                return;
            }
            int idwErrorCode = 0;

            string sUserID = cbUserID3.Text.Trim();
            int iFaceIndex = 50;

            Cursor = Cursors.WaitCursor;
            if (zkemKeeper.DelUserFace(iMachineNumber, sUserID, iFaceIndex))
            {
                zkemKeeper.RefreshData(iMachineNumber);
                MessageBox.Show("DelUserFace,UserID=" + sUserID, "Success");
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }

        private void btnGetUserFace_Click(object sender, EventArgs e)
        {
            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first!", "Error");
                return;
            }

            if (cbUserID3.Text.Trim() == "")
            {
                MessageBox.Show("Please input the UserID first!", "Error");
                return;
            }
            int idwErrorCode = 0;

            string sUserID = cbUserID3.Text.Trim();
            int iFaceIndex = 50;//the only possible parameter value
            int iLength = 128 * 1024;//initialize the length(cannot be zero)
            byte[] byTmpData = new byte[iLength];

            Cursor = Cursors.WaitCursor;
            zkemKeeper.EnableDevice(iMachineNumber, false);

            if (zkemKeeper.GetUserFace(iMachineNumber, sUserID, iFaceIndex, ref byTmpData[0], ref iLength))
            {
                //Here you can manage the information of the face templates according to your request.(for example,you can sava them to the database)
                MessageBox.Show("GetUserFace,the  length of the bytes array byTmpData is " + iLength.ToString(), "Success");
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }

            zkemKeeper.EnableDevice(iMachineNumber, true);
            Cursor = Cursors.Default;
        }

        private void btnGetGeneralLogData_Click(object sender, EventArgs e)
        {
            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first", "Error");
                return;
            }
            int idwErrorCode = 0;

            string sdwEnrollNumber = "";
            int idwVerifyMode = 0;
            int idwInOutMode = 0;
            int idwYear = 0;
            int idwMonth = 0;
            int idwDay = 0;
            int idwHour = 0;
            int idwMinute = 0;
            int idwSecond = 0;
            int idwWorkcode = 0;

            int iGLCount = 0;
            int iIndex = 0;

            Cursor = Cursors.WaitCursor;
            lvLogs.Items.Clear();
            zkemKeeper.EnableDevice(iMachineNumber, false);//disable the device
            if (zkemKeeper.ReadGeneralLogData(iMachineNumber))//read all the attendance records to the memory
            {
                while (zkemKeeper.SSR_GetGeneralLogData(iMachineNumber, out sdwEnrollNumber, out idwVerifyMode,
                            out idwInOutMode, out idwYear, out idwMonth, out idwDay, out idwHour, out idwMinute, out idwSecond, ref idwWorkcode))//get records from the memory
                {
                    iGLCount++;
                    lvLogs.Items.Add(iGLCount.ToString());
                    lvLogs.Items[iIndex].SubItems.Add(sdwEnrollNumber);//modify by Darcy on Nov.26 2009
                    lvLogs.Items[iIndex].SubItems.Add(idwVerifyMode.ToString());
                    lvLogs.Items[iIndex].SubItems.Add(idwInOutMode.ToString());
                    lvLogs.Items[iIndex].SubItems.Add(idwYear.ToString() + "-" + idwMonth.ToString() + "-" + idwDay.ToString() + " " + idwHour.ToString() + ":" + idwMinute.ToString() + ":" + idwSecond.ToString());
                    lvLogs.Items[iIndex].SubItems.Add(idwWorkcode.ToString());
                    iIndex++;
                }
            }
            else
            {
                Cursor = Cursors.Default;
                zkemKeeper.GetLastError(ref idwErrorCode);

                if (idwErrorCode != 0)
                {
                    MessageBox.Show("Reading data from terminal failed,ErrorCode: " + idwErrorCode.ToString(), "Error");
                }
                else
                {
                    MessageBox.Show("No data from terminal returns!", "Error");
                }
            }
            zkemKeeper.EnableDevice(iMachineNumber, true);//enable the device
            Cursor = Cursors.Default;
        }

        private void btnGetDeviceStatus_Click(object sender, EventArgs e)
        {
            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first", "Error");
                return;
            }
            int idwErrorCode = 0;
            int iValue = 0;

            zkemKeeper.EnableDevice(iMachineNumber, false);//disable the device
            if (zkemKeeper.GetDeviceStatus(iMachineNumber, 6, ref iValue)) //Here we use the function "GetDeviceStatus" to get the record's count.The parameter "Status" is 6.
            {
                MessageBox.Show("The count of the AttLogs in the device is " + iValue.ToString(), "Success");
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            zkemKeeper.EnableDevice(iMachineNumber, true);//enable the device
        }

        private void btnClearGLog_Click(object sender, EventArgs e)
        {
            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first", "Error");
                return;
            }
            int idwErrorCode = 0;

            lvLogs.Items.Clear();
            zkemKeeper.EnableDevice(iMachineNumber, false);//disable the device
            if (zkemKeeper.ClearGLog(iMachineNumber))
            {
                zkemKeeper.RefreshData(iMachineNumber);//the data in the device should be refreshed
                MessageBox.Show("All att Logs have been cleared from teiminal!", "Success");
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            zkemKeeper.EnableDevice(iMachineNumber, true);//enable the device
        }

        private void btnRestartDevice_Click(object sender, EventArgs e)
        {
            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first", "Error");
                return;
            }
            int idwErrorCode = 0;

            Cursor = Cursors.WaitCursor;
            if (zkemKeeper.RestartDevice(iMachineNumber) == true)
            {
                MessageBox.Show("The device will restart!", "Success");
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }

        private void btnPowerOffDevice_Click(object sender, EventArgs e)
        {
            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first", "Error");
                return;
            }
            int idwErrorCode = 0;

            Cursor = Cursors.WaitCursor;
            if (zkemKeeper.PowerOffDevice(iMachineNumber))
            {
                MessageBox.Show("PowerOffDevice", "Success");
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }

        private void btnGetDeviceTime_Click(object sender, EventArgs e)
        {
            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first", "Error");
                return;
            }
            int idwErrorCode = 0;

            int idwYear = 0;
            int idwMonth = 0;
            int idwDay = 0;
            int idwHour = 0;
            int idwMinute = 0;
            int idwSecond = 0;

            Cursor = Cursors.WaitCursor;
            if (zkemKeeper.GetDeviceTime(iMachineNumber, ref idwYear, ref idwMonth, ref idwDay, ref idwHour, ref idwMinute, ref idwSecond))
            {
                txtGetDeviceTime.Text = idwYear.ToString() + "-" + idwMonth.ToString() + "-" + idwDay.ToString() + " " + idwHour.ToString() + ":" + idwMinute.ToString() + ":" + idwSecond.ToString();
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }

        private void btnSetDeviceTime_Click(object sender, EventArgs e)
        {
            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first", "Error");
                return;
            }
            int idwErrorCode = 0;

            Cursor = Cursors.WaitCursor;
            if (zkemKeeper.SetDeviceTime(iMachineNumber))
            {
                zkemKeeper.RefreshData(iMachineNumber);//the data in the device should be refreshed
                MessageBox.Show("Successfully set the time of the machine and the terminal to sync PC!", "Success");
                int idwYear = 0;
                int idwMonth = 0;
                int idwDay = 0;
                int idwHour = 0;
                int idwMinute = 0;
                int idwSecond = 0;
                if (zkemKeeper.GetDeviceTime(iMachineNumber, ref idwYear, ref idwMonth, ref idwDay, ref idwHour, ref idwMinute, ref idwSecond))//show the time
                {
                    txtGetDeviceTime.Text = idwYear.ToString() + "-" + idwMonth.ToString() + "-" + idwDay.ToString() + " " + idwHour.ToString() + ":" + idwMinute.ToString() + ":" + idwSecond.ToString();
                }
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }

        private void btnSetDeviceTime2_Click(object sender, EventArgs e)
        {
            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first", "Error");
                return;
            }
            int idwErrorCode = 0;

            int idwYear = Convert.ToInt32(cbYear.Text.Trim());
            int idwMonth = Convert.ToInt32(cbMonth.Text.Trim());
            int idwDay = Convert.ToInt32(cbDay.Text.Trim());
            int idwHour = Convert.ToInt32(cbHour.Text.Trim());
            int idwMinute = Convert.ToInt32(cbMinute.Text.Trim());
            int idwSecond = Convert.ToInt32(cbSecond.Text.Trim());

            Cursor = Cursors.WaitCursor;
            if (zkemKeeper.SetDeviceTime2(iMachineNumber, idwYear, idwMonth, idwDay, idwHour, idwMinute, idwSecond))
            {
                zkemKeeper.RefreshData(iMachineNumber);//the data in the device should be refreshed
                MessageBox.Show("Successfully set the time of the machine as you have set!", "Error");
            }
            else
            {
                zkemKeeper.GetLastError(ref idwErrorCode);
                MessageBox.Show("Operation failed,ErrorCode=" + idwErrorCode.ToString(), "Error");
            }
            Cursor = Cursors.Default;
        }

        private void btnAddUser_Click(object sender, EventArgs e)
        {
            //Done due to an error "Admin Affirm"
            //{
            //    string Id=null;
            //    int Priv=0;
            //    string Pass=null;
            //    string Name= null;
            //    bool Enabled= false;
            //    while((zkemKeeper.SSR_GetAllUserInfo(1,out Id,out Name,out Pass,out Priv,out Enabled)==true))
            //    {
            //        zkemKeeper.SSR_DeleteEnrollData(1, Id, 12);
            //    }

            if (bIsConnected == false)
            {
                MessageBox.Show("Please connect the device first", "Error");
                return;
            }
            int idwErrorCode = 0;
            string EnrollmentNo = txtEnrollmentNo.Text;
            string Password = txtPassword.Text;
            string Name = txtName.Text;
            int iPrivilege = 3;
            bool bEnabled = true;
            if (cbPrivilege.Text == "User")
            {
                iPrivilege = 0;
                setuser(iMachineNumber, EnrollmentNo.ToString(), Name, Password, iPrivilege, bEnabled);

            }
            else if (cbPrivilege.Text == "Administrator")
            {
                iPrivilege = 1;
                setuser(iMachineNumber, EnrollmentNo.ToString(), Name, Password, iPrivilege, bEnabled);
            }
            else
            {
                MessageBox.Show("No/Wrong privilege selected");
            }

        }

        private void btnDeleteAllUsers_Click(object sender, EventArgs e)
        {
            //Done due to an error "Admin Affirm"
            {
                string Id = null;
                int Priv = 1;
                string Pass = null;
                string Name = null;
                bool Enabled = false;
                while ((zkemKeeper.SSR_GetAllUserInfo(1, out Id, out Name, out Pass, out Priv, out Enabled) == true))
                {
                    zkemKeeper.SSR_DeleteEnrollData(1, Id, 12);
                }
                MessageBox.Show("Deleted All User Details");
            }
        }

        private void btnInsertFingerPrint_Click(object sender, EventArgs e)
        {
            setuserfingerprint();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            setuserfaceprint();
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            zkemKeeper.Disconnect();
            Application.Exit();

        }


    }
}