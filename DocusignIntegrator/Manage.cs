//<copyright file="Manage.cs" company="HexaView Technologies Ltd.">
// Copyright (c) 2014 All Rights Reserved
//</copyright>
//<author>Vipul Sharma</author>
//<date>04/15/2015 11:39:58 PM </date>

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.Sql;
using System.Data.SqlClient;
using System.Text;

    public class Manage
    {
        SqlConnection con;
        string ConnectionString = @"Data Source=(LocalDB)\v11.0;AttachDbFilename='" + AppDomain.CurrentDomain.BaseDirectory + "Document.mdf';Integrated Security=True;Connect Timeout=30";
        public int InsertDocument(string EnvelopeID)
        {
            try
            {
                con = new SqlConnection(ConnectionString);
                SqlCommand CmdSql = new SqlCommand("INSERT INTO [tDocuments] (StatusChangedTime, EnvelopeID, Status) VALUES (@Date, @EnvelopeID, @Status)", con);
                con.Open();
                CmdSql.Parameters.AddWithValue("@Date", DateTime.Now);
                CmdSql.Parameters.AddWithValue("@EnvelopeID", EnvelopeID);
                CmdSql.Parameters.AddWithValue("@Status", "Sent");
                
                CmdSql.ExecuteNonQuery();
                con.Close();

                return 1;
            }
            catch(Exception e)
            {
                return 0;
            }
            finally
            {
                con.Close();
            }
       }
        public int UpdateDocument(byte[] SignatureFile, string envelopeId, string status, string declineReason)
        {
            try
            {
                con = new SqlConnection(ConnectionString);
                SqlCommand CmdSql = new SqlCommand("Update tDocuments set SignedDocument=@SignedDocument,Status=@status,StatusChangedTime=@statusChangedTime,DeclineReason=@DeclineReason where EnvelopeID=@EnvelopeID", con);
                con.Open();
                CmdSql.Parameters.AddWithValue("@statusChangedTime", DateTime.Now);
                CmdSql.Parameters.AddWithValue("@EnvelopeID", envelopeId);
                CmdSql.Parameters.AddWithValue("@SignedDocument", SignatureFile);
                CmdSql.Parameters.AddWithValue("@Status", status);
                CmdSql.Parameters.AddWithValue("@DeclineReason", declineReason);
                CmdSql.ExecuteNonQuery();
                con.Close();

                return 1;
            }
            catch (Exception e)
            {
                return 0;
            }
            finally
            {
                con.Close();
            }
        } 
        public DataTable GetUnsignedDocument()
        {
            try
            {
                con = new SqlConnection(ConnectionString);
                SqlCommand CmdSql = new SqlCommand("select Status,EnvelopeID,StatusChangedTime from tDocuments where Status<>'declined' and Status<>'completed'", con);
                con.Open();
                SqlDataAdapter dr = new SqlDataAdapter(CmdSql);
                DataTable dt = new DataTable();
                dr.Fill(dt);
                con.Close();
                return dt;
            }
            catch (Exception e)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
        }
        public byte[] GetDocumentData(string EnvelopeID)
        {
            try
            {
                byte[] Data;
                con = new SqlConnection(ConnectionString);
                SqlCommand CmdSql = new SqlCommand("select SignedDocument from tDocuments where EnvelopeID=@EnvelopeID", con);
                con.Open();
                CmdSql.Parameters.AddWithValue("@EnvelopeID", EnvelopeID);
                Data = Encoding.ASCII.GetBytes(CmdSql.ExecuteScalar().ToString());
                con.Close();
                return Data;
            }
            catch (Exception e)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
        }
        public DataTable GetDocuments()
        {
            try
            {
                con = new SqlConnection(ConnectionString);
                SqlCommand CmdSql = new SqlCommand("select Status,EnvelopeID,StatusChangedTime,SignedDocument from tDocuments", con);
                con.Open();
                SqlDataAdapter dr = new SqlDataAdapter(CmdSql);
                DataTable dt = new DataTable();
                dr.Fill(dt);
                con.Close();
                return dt;
            }
            catch (Exception e)
            {
                return null;
            }
            finally
            {
                con.Close();
            }
        }
    }