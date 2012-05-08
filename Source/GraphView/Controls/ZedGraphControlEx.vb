﻿Imports System.Windows.Forms
Imports ZedGraph

Namespace Controls
    Public Class ZedGraphControlEx
        Inherits ZedGraphControl

        Public Sub ZoomIn()
            ZedGraphControl_MouseWheel(Me, New MouseEventArgs(Windows.Forms.MouseButtons.None,
                                                              1, Width / 2.0, Height / 2.0, 1.0))
        End Sub

    End Class
End Namespace