Imports System.Threading
Imports System.ComponentModel
Imports System.IO
Imports System.Security.Cryptography


Public Class Main_Screen
    Private mCSP As SymmetricAlgorithm

    Private optDES As Boolean = False
    Private optTripleDES As Boolean = True
    Private Key As String = ""
    Private IV As String = ""

    Private Function SetEnc() As SymmetricAlgorithm
        If optDES = True Then
            Return New DESCryptoServiceProvider
        Else
            If optTripleDES = True Then
                Return New TripleDESCryptoServiceProvider
            End If
        End If
        Return New TripleDESCryptoServiceProvider
    End Function

    Private Sub GenerateKey()
        mCSP = SetEnc()
        mCSP.GenerateKey()
        Key = Convert.ToBase64String(mCSP.Key)
    End Sub

    Private Sub GenerateIV()
        mCSP.GenerateIV()
        IV = Convert.ToBase64String(mCSP.IV)
    End Sub

    Private Function EncryptString(ByVal Value As String) As String
        Dim ct As ICryptoTransform
        Dim ms As MemoryStream
        Dim cs As CryptoStream
        Dim byt() As Byte

        ct = mCSP.CreateEncryptor(mCSP.Key, mCSP.IV)

        byt = System.Text.Encoding.UTF8.GetBytes(Value)

        ms = New MemoryStream
        cs = New CryptoStream(ms, ct, CryptoStreamMode.Write)
        cs.Write(byt, 0, byt.Length)
        cs.FlushFinalBlock()

        cs.Close()

        Return Convert.ToBase64String(ms.ToArray())
    End Function

    Private Function DecryptString(ByVal Value As String) _
     As String
        Dim ct As ICryptoTransform
        Dim ms As MemoryStream
        Dim cs As CryptoStream
        Dim byt() As Byte

        ct = mCSP.CreateDecryptor(mCSP.Key, mCSP.IV)

        byt = Convert.FromBase64String(Value)

        ms = New MemoryStream
        cs = New CryptoStream(ms, ct, CryptoStreamMode.Write)
        cs.Write(byt, 0, byt.Length)
        cs.FlushFinalBlock()

        cs.Close()

        Return System.Text.Encoding.UTF8.GetString(ms.ToArray())
    End Function


    Private Sub Error_Handler(ByVal ex As Exception, Optional ByVal identifier_msg As String = "")
        Try
            If ex.Message.IndexOf("Thread was being aborted") < 0 Then
                Dim Display_Message1 As New Display_Message()
                Display_Message1.Message_Textbox.Text = "The Application encountered the following problem: " & vbCrLf & identifier_msg & ": " & ex.Message.ToString
                'Display_Message1.Message_Textbox.Text = "The Application encountered the following problem: " & vbCrLf & identifier_msg & ": " & ex.ToString
                Display_Message1.Timer1.Interval = 1000
                Display_Message1.ShowDialog()
                Dim dir As System.IO.DirectoryInfo = New System.IO.DirectoryInfo((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs")
                If dir.Exists = False Then
                    dir.Create()
                End If
                dir = Nothing
                Dim filewriter As System.IO.StreamWriter = New System.IO.StreamWriter((Application.StartupPath & "\").Replace("\\", "\") & "Error Logs\" & Format(Now(), "yyyyMMdd") & "_Error_Log.txt", True)
                filewriter.WriteLine("#" & Format(Now(), "dd/MM/yyyy hh:mm:ss tt") & " - " & identifier_msg & ": " & ex.ToString)
                filewriter.WriteLine(" ")
                filewriter.Flush()
                filewriter.Close()
                filewriter = Nothing
            End If
        Catch exc As Exception
            MsgBox("An error occurred in the application's error handling routine. The application will try to recover from this serious error.", MsgBoxStyle.Critical, "Critical Error Encountered")
        End Try
    End Sub




    Private Sub Main_Screen_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load
        Try
            Me.Text = My.Application.Info.ProductName
            Label4.Text = "Build " & Format(My.Application.Info.Version.Major, "0000") & Format(My.Application.Info.Version.Minor, "00") & Format(My.Application.Info.Version.Build, "00") & "." & Format(My.Application.Info.Version.Revision, "00")
            If My.Computer.FileSystem.FileExists((Application.StartupPath & "\encryption.txt").Replace("\\", "\")) = True Then
                Dim reader As StreamReader = New StreamReader((Application.StartupPath & "\encryption.txt").Replace("\\", "\"), System.Text.Encoding.UTF8)
                Dim variables As ArrayList = New ArrayList
                While reader.Peek <> -1
                    variables.Add(reader.ReadLine())
                End While
                reader.Close()
                reader = Nothing
                If variables.Count >= 4 Then
                    Key = variables(0)
                    IV = variables(1)
                    mCSP = SetEnc()
                    mCSP.Key = Convert.FromBase64String(Key)
                    mCSP.IV = Convert.FromBase64String(IV)
                    username.Text = DecryptString(variables(2))
                    password.Text = DecryptString(variables(3))
                End If
                variables = Nothing
            End If
        Catch ex As Exception
            Error_Handler(ex, "Proxy Form Load")
        End Try
    End Sub


    Private Sub HelpToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles HelpToolStripMenuItem.Click
        Try
            HelpBox1.ShowDialog()
        Catch ex As Exception
            Error_Handler(ex, "Display Help Screen")
        End Try
    End Sub

    Private Sub AboutToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles AboutToolStripMenuItem.Click
        Try
            AboutBox1.ShowDialog()
        Catch ex As Exception
            Error_Handler(ex, "Display About Screen")
        End Try
    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click
        Try
            mCSP = SetEnc()
            mCSP.GenerateKey()
            mCSP.GenerateIV()

            Dim writer As StreamWriter = New StreamWriter((Application.StartupPath & "\encryption.txt").Replace("\\", "\"), False, System.Text.Encoding.UTF8)
            writer.WriteLine(Convert.ToBase64String(mCSP.Key))
            writer.WriteLine(Convert.ToBase64String(mCSP.IV))
            writer.WriteLine(EncryptString(username.Text))
            writer.WriteLine(EncryptString(password.Text))
            writer.Flush()
            writer.Close()
            writer = Nothing

            If My.Computer.FileSystem.FileExists((Application.StartupPath & "\encryption.txt").Replace("\\", "\")) = True Then
                Process.Start((Application.StartupPath & "\encryption.txt").Replace("\\", "\"))
            End If

        Catch ex As Exception
            Error_Handler(ex, "Encrypting Data")
        End Try
    End Sub

    Private Sub RestartToolStripMenuItem_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles RestartToolStripMenuItem.Click
        Try
            Me.Close()
            Process.Start(Application.ExecutablePath)
        Catch ex As Exception
            Error_Handler(ex, "Restart Application")
        End Try
    End Sub
End Class
