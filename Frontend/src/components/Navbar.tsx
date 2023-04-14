import { useCallback, useState, useContext, useEffect,Fragment } from "react";
import { Link } from "react-router-dom";
import AppBar from '@mui/material/AppBar';
import Box from '@mui/material/Box';
import Toolbar from '@mui/material/Toolbar';
import IconButton from '@mui/material/IconButton';
import Typography, { TypographyProps } from '@mui/material/Typography';
import Menu from '@mui/material/Menu';
import MenuIcon from '@mui/icons-material/Menu';
import HomeIcon from "@mui/icons-material/Home";
import BookIcon from "@mui/icons-material/Book";
import Button from '@mui/material/Button';
import MenuItem from '@mui/material/MenuItem';
import LocalCafe from '@mui/icons-material/LocalCafe';

const menuItems = [
    {
        link: "/Chat",
        name: "Chat",
        requireAuth: false,
        icon: <BookIcon className="text-white" />
    },
    {
        link: "/Bot",
        name: "Bot",
        requireAuth: false,
        icon: <HomeIcon className="text-white" />

    },
    {
        link: "/Admin",
        name: "Admin",
        requireAuth: true,
        icon: <BookIcon className="text-white" />
    }
];


const MenuButton = (props: any) => {
    const { navOpenHandler, navCloseHandler, navState, ...restProps } = props;
    return (
        <>
            <IconButton
                size="large"
                aria-label="menua"
                aria-controls="menu-appbar"
                aria-haspopup="true"
                onClick={navOpenHandler}
                color="inherit"
                sx={{
                    display:
                    {
                        xs: 'flex',
                        md: 'none',
                    },
                }}
            >
                <MenuIcon />
            </IconButton>
            <Menu
                id="menu-appbar"
                anchorEl={navState}
                anchorOrigin={{
                    vertical: 'bottom',
                    horizontal: 'left',
                }}
                keepMounted
                transformOrigin={{
                    vertical: 'top',
                    horizontal: 'left',
                }}
                open={Boolean(navState)}
                onClose={navCloseHandler}
                sx={{
                    display: { xs: 'block', md: 'none' },
                }}
            >
                {menuItems.map((element) => (
                        <MenuItem key={"menuItem-" + element.name} onClick={navCloseHandler}>
                            <Link to={element.link} key={"link-" + element.name} style={{ textDecoration: 'none' }} >
                                {element.name}
                            </Link>
                        </MenuItem>
                ))}
            </Menu>
        </>

    );
}

const SiteTitle = (props: any) => {
    const { children, navCloseHandler, to } = props;
    return (
        <>
            <LocalCafe
                sx={{
                    display: { xs: 'none', md: 'flex' },
                    mr: 1
                }}
            />
            <Link to={to} style={{ textDecoration: 'none', display: 'contents' }}>
                <Button variant="text"
                    onClick={navCloseHandler}
                    sx={{ color: 'white', ':hover': { color: 'yellow' }, fontSize: 'large' }}
                >
                    {children}
                </Button>

            </Link>
        </>
    );
};
const SiteBar = (props: any) => {
    const { navCloseHandler } = props;
    const [selectedButton, setSelectedButton] = useState(""); // add state variable
    const handleButtonClick = (name: string) => {
        setSelectedButton(name); // set selected button
        navCloseHandler(); // close nav menu
    };
    return (
        <Box sx={{ ml: 20, flexGrow: 1, display: { xs: 'none', md: 'flex' } }}>
            {menuItems.map((element) => (
                    <Link to={element.link} key={"link-" + element.name} style={{ textDecoration: 'none' }}>
                        <Button variant="text"
                            key={"button-" + element.name}
                            onClick={() => handleButtonClick(element.name)} // handle button click
                            sx={{
                                my: 2,
                                color: selectedButton === element.name ? 'ActiveCaption' : 'white', // set color based on selected button
                                display: 'block',
                                ':hover': { color: 'yellow' }
                            }}
                        >
                            {element.name}
                        </Button>
                    </Link>

            ))}
        </Box>
    )
}



const Navbar = () => {
    const [anchorElNav, setAnchorElNav] = useState<null | HTMLElement>(null);
    const handleOpenNavMenu = useCallback((event: React.MouseEvent<HTMLElement>) => {
        setAnchorElNav(event.currentTarget);
    }, [setAnchorElNav]);
    const handleCloseNavMenu = useCallback(() => {
        setAnchorElNav(null);
    }, [setAnchorElNav]);
    return (
        <>
            <AppBar position="static">
                <Toolbar>
                    <MenuButton navOpenHandler={handleOpenNavMenu} navCloseHandler={handleCloseNavMenu} navState={anchorElNav} />
                    <SiteTitle navCloseHandler={handleCloseNavMenu} to="/" >Private Domain Azure GPT </SiteTitle>
                    <SiteBar navCloseHandler={handleCloseNavMenu} />
                    <Box sx={{ flexGrow: 1 }}></Box>
                </Toolbar>
            </AppBar>
        </>
    );
};
export default Navbar;
