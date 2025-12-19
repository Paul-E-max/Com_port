# Thorlabs Power Meter App - Changelog

## v1.1.0 (2024-12-19)

### New Features
- **Real-time Power Graph**: Added live power visualization with auto-scaling
- **Time Range Selection**: View 30 seconds, 60 seconds, or 5 minutes of history
- **Statistics Overlay**: Min/Max/Avg lines displayed on chart
- **Dark Theme Chart**: Chart matches application dark theme

### UI Changes
- Expanded window size to accommodate chart panel (900x560)
- Added chart toggle checkbox
- Added time range buttons (30s, 60s, 5m)

### Files Added
- `Controls/PowerMeterChart.cs` - Custom GDI+ chart control

### Files Modified
- `MainForm.cs` - Chart integration and version headers

---

## v1.0.0 (2024-12-18)

### Initial Release
- WinUSB/TLPMX driver support (no NI-VISA required)
- NI-VISA fallback mode
- Real-time power measurement display
- Statistics tracking (Min/Max/Avg)
- Data logging to CSV
- Configurable settings: Wavelength, Beam Diameter, Averaging
- Unit conversion (W, mW, µW, nW, dBm)
- Dark theme UI
