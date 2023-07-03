using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;

public partial class TicketSimulator : Window
{
    private string connectionString = ConfigurationManager.ConnectionStrings["YourConnectionString"].ConnectionString;

    public TicketSimulator()
    {
        InitializeComponent();

        LoadCities();
    }

    private void LoadCities()
    {
        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT CityID, CityName FROM Cities";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                DataTable citiesTable = new DataTable();
                adapter.Fill(citiesTable);

                cmbCity.ItemsSource = citiesTable.DefaultView;
                cmbCity.DisplayMemberPath = "CityName";
                cmbCity.SelectedValuePath = "CityID";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error occurred while loading cities: " + ex.Message);
        }
    }

    private void cmbCity_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int selectedCityID = (int)cmbCity.SelectedValue;

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT AirplaneID, AirplaneName, PilotName FROM Airplanes WHERE CityID = @CityID";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.SelectCommand.Parameters.AddWithValue("@CityID", selectedCityID);
                DataTable airplanesTable = new DataTable();
                adapter.Fill(airplanesTable);

                cmbAirplane.ItemsSource = airplanesTable.DefaultView;
                cmbAirplane.DisplayMemberPath = "AirplaneName";
                cmbAirplane.SelectedValuePath = "AirplaneID";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error occurred while loading airplanes: " + ex.Message);
        }
    }

    private void cmbAirplane_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        int selectedAirplaneID = (int)cmbAirplane.SelectedValue;

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT PilotName FROM Airplanes WHERE AirplaneID = @AirplaneID";
                SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                adapter.SelectCommand.Parameters.AddWithValue("@AirplaneID", selectedAirplaneID);
                DataTable pilotsTable = new DataTable();
                adapter.Fill(pilotsTable);

                if (pilotsTable.Rows.Count > 0)
                {
                    txtPilotInfo.Text = pilotsTable.Rows[0]["PilotName"].ToString();
                }
                else
                {
                    txtPilotInfo.Text = string.Empty;
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error occurred while loading pilot information: " + ex.Message);
        }
    }

    private void btnPurchase_Click(object sender, RoutedEventArgs e)
    {
        int selectedCityID = (int)cmbCity.SelectedValue;
        int selectedAirplaneID = (int)cmbAirplane.SelectedValue;
        int selectedScheduleID = (int)cmbSchedule.SelectedValue;
        string flightType = cmbFlightType.SelectedItem.ToString();

        double distance = GetDistance(selectedCityID);
        double price = GetPrice(distance, flightType);

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Tickets (CityID, AirplaneID, ScheduleID, FlightType, Price) " +
                               "VALUES (@CityID, @AirplaneID, @ScheduleID, @Flight";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CityID", selectedCityID);
                command.Parameters.AddWithValue("@AirplaneID", selectedAirplaneID);
                command.Parameters.AddWithValue("@ScheduleID", selectedScheduleID);
                command.Parameters.AddWithValue("@FlightType", flightType);
                command.Parameters.AddWithValue("@Price", price);

                connection.Open();
                int rowsAffected = command.ExecuteNonQuery();
                connection.Close();

                if (rowsAffected > 0)
                {
                    MessageBox.Show("Ticket purchased successfully!");
                }
                else
                {
                    MessageBox.Show("Failed to purchase the ticket. Please try again.");
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error occurred while purchasing the ticket: " + ex.Message);
        }
    }

    private double GetDistance(int cityID)
    {
        double distance = 0;

        try
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT Distance FROM Cities WHERE CityID = @CityID";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CityID", cityID);

                connection.Open();
                distance = Convert.ToDouble(command.ExecuteScalar());
                connection.Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("Error occurred while calculating distance: " + ex.Message);
        }

        return distance;
    }

    private double GetPrice(double distance, string flightType)
    {
        double price = 0;

        switch (flightType)
        {
            case "Economy":
                price = distance * 2;
                break;
            case "Business":
                price = distance * 4;
                break;
            case "Premium":
                price = distance * 8;
                break;
        }

        return price;
    }
}
