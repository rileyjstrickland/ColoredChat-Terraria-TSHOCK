Imports MySql.Data.MySqlClient
Imports System.ComponentModel
Imports System.Windows.Forms
Imports TShockAPI
Imports Terraria
Namespace ChatHookTutorial
    <APIVersion(1, 11)>
    Public Class ColoredText
        Inherits TerrariaPlugin
        Dim data As MySqlDataReader
        Dim commanddo As New MySqlCommand
        Dim con As New MySqlConnection
        Dim savecolor As Boolean = False
        Dim colorarray As Array
        Dim constrarray As Array
#Region "information"
        Public Overrides ReadOnly Property Version() As Version
            Get
                Return New Version("1.0.0.0")
            End Get
        End Property

        Public Overrides ReadOnly Property Name() As String
            Get
                Return "Colors!"
            End Get
        End Property

        Public Overrides ReadOnly Property Author() As String
            Get
                Return "Hex Hooves"
            End Get
        End Property

        Public Overrides ReadOnly Property Description() As String
            Get
                Return "Allow For Chatting In ANY Color, Per User!"
            End Get
        End Property
#End Region
        Public Sub New(game As Main)
            MyBase.New(game)
            Order = -1
            Try
                If My.Computer.FileSystem.FileExists(Application.StartupPath & "\ServerPlugins\colors\config.cfg") = False Then
                    If My.Computer.FileSystem.DirectoryExists(Application.StartupPath & "\ServerPlugins\colors\") = False Then
                        My.Computer.FileSystem.CreateDirectory(Application.StartupPath & "\ServerPlugins\colors\")
                    End If
                    System.IO.File.Create(Application.StartupPath & "\ServerPlugins\colors\config.cfg").Dispose()
                    My.Computer.FileSystem.WriteAllText(Application.StartupPath & "\ServerPlugins\colors\config.cfg", "server,username,password,database,table", False)
                    MsgBox("Colors Config Created")
                End If
                constrarray = My.Computer.FileSystem.ReadAllText(Application.StartupPath & "\ServerPlugins\colors\config.cfg").Split(",")
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try
        End Sub
        Public Overrides Sub Initialize()

            con.ConnectionString = "Server=" & constrarray(0) & ";User Id=" & constrarray(1) & ";Password=" & constrarray(2) & ";Database=" & constrarray(3) & ";"
            con.Open()
            commanddo.Connection = con
            AddHandler Hooks.ServerHooks.Chat, AddressOf OnChat
        End Sub
        Private Sub OnChat(msg As messageBuffer, who As Integer, message As String, args As HandledEventArgs)
            Dim Player As TSPlayer = TShock.Players(msg.whoAmI)
            If savecolor Then
                If message.ToLower = "yes" Then
                    Try
                        commanddo.CommandText = "select * FROM `" & constrarray(4) & "` WHERE `ip`='" & Player.IP & "'"
                        data = commanddo.ExecuteReader
                        data.Read()
                        If data.HasRows = 0 Then
                            data.Close()
                            commanddo.CommandText = "INSERT INTO `" & constrarray(4) & "` (`ip`, `R`, `G`, `B`) VALUES ('" & Player.IP & "', " & colorarray(0) & ", " & colorarray(1) & ", " & colorarray(2) & ");"
                            commanddo.ExecuteScalar()
                        Else
                            data.Close()
                            commanddo.CommandText = "UPDATE `" & constrarray(4) & "` SET `R`=" & colorarray(0) & ", `G`=" & colorarray(1) & ",`B`=" & colorarray(2) & " WHERE `ip`='" & Player.IP & "';"
                            commanddo.ExecuteScalar()
                        End If
                        Player.SendMessage("Saved!")
                    Catch ex As Exception

                    End Try
                    savecolor = False
                Else
                    Player.SendMessage("Save Canceled!!")
                    savecolor = False
                End If
                args.Handled = True
            Else
                Try
                    Dim cmd As String = message.Split()(0).Substring(1).ToLower
                    Select Case cmd
                        Case "color", "colour"
                            Try
                                If message.Split()(1) = "help" Then
                                    Player.SendMessage("Colors Are In RGB Format, From 0 - 255")
                                    Player.SendMessage("Example (For Red) : /color 255,0,0")
                                    args.Handled = True
                                Else
                                    Try
                                        If message.Split()(1).Split(",").Count <> 3 Then
                                            Player.SendMessage("Error In Syntax")
                                            args.Handled = True
                                        Else
                                            colorarray = message.Split()(1).Split(",")
                                            Dim color As New Color
                                            Dim invalidvalue As Boolean = False
                                            If IsNumeric(colorarray(0)) = False Then
                                                Player.SendMessage(colorarray(0) & " is not valid for value Red.  Correct values are 0 - 255")
                                                invalidvalue = 1
                                            End If
                                            If IsNumeric(colorarray(1)) = False Then
                                                Player.SendMessage(colorarray(1) & " is not valid for value Green.  Correct values are 0 - 255")
                                                invalidvalue = 1
                                            End If
                                            If IsNumeric(colorarray(2)) = False Then
                                                Player.SendMessage(colorarray(2) & " is not valid for value Blue.  Correct values are 0 - 255")
                                                invalidvalue = 1
                                            End If
                                            If invalidvalue Then
                                                args.Handled = True
                                            Else
                                                If colorarray(0) > 255 Then
                                                    colorarray(0) = 255
                                                ElseIf colorarray(0) < 0 Then
                                                    colorarray(0) = 0
                                                End If
                                                If colorarray(1) > 255 Then
                                                    colorarray(1) = 255
                                                ElseIf colorarray(1) < 0 Then
                                                    colorarray(1) = 0
                                                End If
                                                If colorarray(2) > 255 Then
                                                    colorarray(2) = 255
                                                ElseIf colorarray(2) < 0 Then
                                                    colorarray(2) = 0
                                                End If
                                                color.R = colorarray(0)
                                                color.G = colorarray(1)
                                                color.B = colorarray(2)
                                                Player.SendMessage("Preview", color)
                                                Player.SendMessage("Save Color? (Yes/No)")
                                                savecolor = True
                                                args.Handled = True
                                            End If
                                        End If
                                    Catch nocomma As IndexOutOfRangeException
                                        Player.SendMessage("Error In Syntax")
                                        args.Handled = True
                                    Catch ex As Exception
                                        Console.WriteLine(ex.Message)
                                    End Try
                                End If
                            Catch nocomma As IndexOutOfRangeException
                                Player.SendMessage("Error In Syntax")
                                args.Handled = True
                            Catch ex As Exception
                                Console.WriteLine(ex.Message)
                            End Try
                        Case "help"
                            If message.Split().Count > 1 Then
                                If message.Split()(1).ToLower = 1 Then
                                    Player.SendMessage("Use /color help for color help.", Color.Red)
                                End If
                            Else
                                Player.SendMessage("Use /color help for color help.", Color.Red)
                            End If
                    End Select
                    If message.StartsWith("/") Then
                    Else
                        Dim color As New Color
                        commanddo.CommandText = "select * FROM `" & constrarray(4) & "` WHERE `ip`='" & Player.IP & "'"
                        data = commanddo.ExecuteReader
                        data.Read()
                        If data.HasRows = 0 Then
                            data.Close()
                            color.R = 255
                            color.G = 255
                            color.B = 255
                        Else
                            color.R = data.Item(1)
                            color.G = data.Item(2)
                            color.B = data.Item(3)
                            data.Close()
                        End If
                        TSPlayer.All.SendMessage(Player.Group.Prefix & Player.Name & Player.Group.Suffix & ": " & message, color)
                        args.Handled = True
                    End If
                Catch ex As Exception
                    Console.WriteLine(ex.GetType)
                End Try
            End If
        End Sub
        Protected Overrides Sub Dispose(disposing As Boolean)
            ' Ensure that we are actually disposing.
            '             

            If disposing Then
                ' Using the -= operator, we remove our method
                '                 * from the hook.
                '                 
                con.Close()
                RemoveHandler Hooks.ServerHooks.Chat, AddressOf OnChat
            End If
            MyBase.Dispose(disposing)
        End Sub
    End Class
End Namespace
