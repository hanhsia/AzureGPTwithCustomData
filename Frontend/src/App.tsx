import { useState } from "react";
import { Routes, Route } from "react-router-dom";
import Navbar from "./components/Navbar";
import Bot from "./pages/Bot/Bot"
import Admin from "./pages/Admin/Admin";
import NoPage from "./pages/NoPage/NoPage";
import Chat from "./pages/Chat/Chat"
import { Container, Box, Grid, TextField, Typography, Button } from '@mui/material';

const Pages = () => {
    return (
        <Routes>
            <Route path="/" element={<Chat />} />
            <Route path="/bot" element={<Bot />} />
            <Route path="/chat" element={<Chat />} />
            <Route path="/admin" element={<Admin />} />
            <Route path="*" element={<NoPage />} />
        </Routes>
    )
}

const App = (props: any) => {
    const { pca } = props;
    const [isLogin, setIsLogin] = useState(false)
    const [userName, setUserName] = useState('');
    const [password, setPassword] = useState('');


    const onLogin = () => {
        if (userName === "admin" && password === "Microsoft123.") {
            setIsLogin(true);
        }
    }
    const onUserNameChange = (
        event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
    ) => {
        const newValue = event.target.value;
        setUserName(newValue);
    };

    const onPasswordChange = (
        event: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>
    ) => {
        const newValue = event.target.value;
        setPassword(newValue);
    };
    return (isLogin?

        <Box sx={{ display: 'flex', flexDirection: 'column', height: '100vh' }}>
            <Navbar />
            <Pages />
        </Box>
        :
        (
            <Container maxWidth="xs">
                <Box sx={{ mt: 8, mb: 4 }}>
                    <Typography variant="h4" align="center">
                        Log in
                    </Typography>
                </Box>
                <Box component="form" noValidate>
                    <TextField
                        fullWidth
                        autoFocus
                        margin="dense"
                        name="UserName"
                        label="UserName"
                        type="text"
                        value={userName}
                        onChange={onUserNameChange}
                        required
                    />
                    <TextField
                        fullWidth
                        autoFocus
                        margin="dense"
                        name="Password"
                        label="Password"
                        type="password"
                        value={password}
                        onChange={onPasswordChange}
                        required
                    />
                    <Box sx={{ mt: 2 }}>
                        <Grid container justifyContent="center">
                            <Grid item>
                                <Button variant="contained" color="primary" onClick={onLogin}>
                                    Login
                                </Button>
                            </Grid>
                        </Grid>
                    </Box>
                </Box>
            </Container>

        )
    );
};

export default App;