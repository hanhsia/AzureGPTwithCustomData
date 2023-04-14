import { useState, useCallback } from 'react';
import {
    Box,
    List,
    ListItemButton,
    ListItem,
    ListItemIcon,
    ListItemText,
    IconButton,
    Divider,
} from '@mui/material';
import { ChevronLeft, ChevronRight } from '@mui/icons-material';
import { SvgIconComponent } from '@mui/icons-material';  

interface SidebarItemProps {
    item: Item;
    open: boolean;
    currentItem: string;
    onClick: (itemId: string) => void;
}  
interface SidebarProps {
    items: any[];
    currentItem: string;
    handleItemClick: (itemId: string) => void;
}

interface Item {
    id: string;
    title: string;
    icon: React.ReactElement<SvgIconComponent>;
}  

const SidebarItem: React.FC<SidebarItemProps> = ({
    item,
    open,
    currentItem,
    onClick,
}) => {
    return (
        <ListItem key={item.id} disablePadding sx={{ display: 'block' }}>
            <ListItemButton
                sx={{
                    minHeight: 48,
                    justifyContent: open ? 'initial' : 'center',
                    px: 2,
                }}
                onClick={() => onClick(item.id)}
                selected={currentItem === item.id}
            >
                <ListItemIcon
                    sx={{
                        minWidth: 0,
                        mr: open ? 2 : 'auto',
                        justifyContent: 'center',
                    }}
                >
                    {item.icon}
                </ListItemIcon>
                <ListItemText
                    primary={item.title}
                    sx={{ display: open ? 'block' : 'none', opacity: open ? 1 : 0 }}
                />
            </ListItemButton>
        </ListItem>
    );
};  

const Sidebar: React.FC<SidebarProps> = ({
    items,
    currentItem,
    handleItemClick,
}) => {
    const [open, setOpen] = useState(false);
    const [width, setWidth] = useState(56);
    const handleOpen = useCallback(() => {
        setOpen(true);
        setWidth(200);
    }, []);
    const handleClose = useCallback(() => {
        setOpen(false);
        setWidth(56);
    }, []);  


    return (
        <Box
            width={{ width }}
            sx={{
                borderRight: 2,
                borderColor: 'grey.500',
                bgcolor: 'grey.200',
            }}
        >
            <Box
                style={{
                    display: 'flex',
                    alignItems: 'right',
                    justifyContent: 'flex-end',
                }}
            >
                <IconButton
                    color="inherit"
                    edge="start"
                    sx={{
                        mr: 1,
                        ml: 1,
                    }}
                    onClick={open ? handleClose : handleOpen}
                >
                    {open && <ChevronLeft />}
                    {!open && <ChevronRight />}
                </IconButton>
            </Box>
            <Divider />
            <List>
                {items.map((item) => (
                    <SidebarItem
                        key={item.id}
                        item={item}
                        open={open}
                        currentItem={currentItem}
                        onClick={handleItemClick}
                    />
                ))}
            </List>
        </Box>
    );
};  

export default Sidebar;  