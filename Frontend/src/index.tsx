import { StrictMode } from 'react';
import { createRoot } from 'react-dom/client';
import { ThemeProvider } from '@mui/material/styles';
import { BrowserRouter as Router } from "react-router-dom";
import { theme } from "./styles/theme";
import App from './App';
//import reportWebVitals from './reportWebVitals';


const root = createRoot(
    document.getElementById('root') as HTMLElement
);
root.render(
    <StrictMode>
        <Router>
            <ThemeProvider theme={theme}>
                <App />
            </ThemeProvider>
        </Router>
    </StrictMode>
);

// If you want to start measuring performance in your app, pass a function
// to log results (for example: reportWebVitals(console.log))
// or send to an analytics endpoint. Learn more: https://bit.ly/CRA-vitals
//reportWebVitals();
