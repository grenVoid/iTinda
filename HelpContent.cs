using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POS
{
    internal static class HelpContent
    {
        public static string GetHelpGuide()
        {
            return
@"{\rtf1\ansi\deff0
{\fonttbl
{\f0\fnil\fcharset0 Segoe UI;}
}
{\colortbl;
\red210\green180\blue140;
}

\viewkind4\uc1
\cf1

\pard\qc\b\f0\fs40 iTINDA\b0\par
\pard\qc\b\fs22 POINT OF SALE SYSTEM\b0\par
\par


\pard\ql\b\fs24 ABOUT iTINDA\b0\par
\pard\ql\fs20
\tab iTinda is a Point-of-Sale (POS) application designed for small retail businesses. It helps manage products, inventory, sales transactions, receipts, and daily sales reports in a simple and user-friendly system.
\par
\par


\pard\ql\b\fs24 KEYBOARD SHORTCUTS\b0\par
\pard\ql\fs20
\tab\b F1\b0             - Enable Account Text Bar\par
\tab\b F2\b0             - Enable Print and Cash Given/Payment Cash\par
\tab\b F3\b0             - Clear Data Transaction\par
\tab\b Esc\b0            - Transaction Data Management\par
\par


\pard\ql\b\fs24 SALES TRANSACTION\b0\par
\pard\ql\fs20
\tab 1. Select or search for a product.\par
\tab 2. Enter the desired quantity.\par
\tab 3. Review the items and quantities.\par
\tab 4. Check the total amount.\par
\tab 5. Confirm the transaction.\par
\tab 6. Print the customer's receipt.\par
\par


\pard\ql\b\fs24 INVENTORY\b0\par
\pard\ql\fs20
\tab The inventory section allows the user to:\par
\par
\tab • Add new products\par
\tab • Update product information\par
\tab • Monitor available stock\par
\tab • Update product prices\par
\tab • Review product details\par
\par
\tab Always make sure that product information and stock quantity are accurate before processing sales.\par
\par


\pard\ql\b\fs24 DAILY SALES REPORT\b0\par
\pard\ql\fs20
\tab At the end of the business transaction:\par
\par
\tab 1. Select End Transaction.\par
\tab 2. The system records the day's sales information.\par
\tab 3. Review the Daily Sales Report.\par
\tab 4. Select the appropriate printer.\par
\tab 5. Print the report.\par
\par

\tab The report contains:\par
\par
\tab • Username\par
\tab • Date\par
\tab • Time In\par
\tab • Time Out\par
\tab • Exit Reason\par
\tab • Total Receipts\par
\tab • Individual Receipt Amounts\par
\tab • Total Sales\par
\par


\pard\ql\b\fs24 SECURITY\b0\par
\pard\ql\fs20
\tab Certain sensitive functions require OTP verification.\par
\par
\tab The OTP is sent to the registered email address of the account.\par
\par
\tab The entered contact information must match the currently logged-in account before an OTP can be sent.\par
\par
\tab\b DO NOT share your OTP with anyone.\b0\par
\par


\pard\ql\b\fs24 OTP TROUBLESHOOTING\b0\par
\pard\ql\fs20
\tab If the OTP is not received:\par
\par
\tab • Check your internet connection.\par
\tab • Check your email address.\par
\tab • Check the Spam or Junk folder.\par
\tab • Wait a few moments for the email to arrive.\par
\tab • Request a new OTP if the previous OTP has expired.\par
\par


\pard\ql\b\fs24 RECEIPT PRINTER\b0\par
\pard\ql\fs20
\tab If the receipt does not print:\par
\par
\tab 1. Make sure the printer is powered on.\par
\tab 2. Check the USB connection.\par
\tab 3. Make sure paper is properly installed.\par
\tab 4. Check that the correct printer is selected.\par
\tab 5. Make sure the printer is available in Windows.\par
\par


\pard\ql\b\fs24 APP COMPATIBILITY\b0\par
\pard\ql\fs20

\tab\b Operating System:\b0\par
\tab Windows 10 / Windows 11\par
\par

\tab\b Database:\b0\par
\tab SQLite\par
\par

\tab\b Display:\b0\par
\tab 1366 × 768 or higher recommended\par
\par

\tab\b Printer:\b0\par
\tab Windows-compatible thermal receipt printer\par
\par

\tab\b Internet:\b0\par
\tab Required for email-based OTP verification.\par
\par


\pard\ql\b\fs24 IMPORTANT REMINDERS\b0\par
\pard\ql\fs20
\tab • Always review the customer's order before confirming.\par
\tab • Make sure the quantity is correct.\par
\tab • Verify the total amount before completing a sale.\par
\tab • Keep inventory information accurate.\par
\tab • Do not share your OTP.\par
\tab • Do not close the application while a transaction is still being processed.\par
\par


\pard\ql\b\fs24 END OF HELP GUIDE\b0\par

\pard\qc\fs20
For additional assistance, contact the system administrator.
\pard\ql
}";
        }
    }
}
