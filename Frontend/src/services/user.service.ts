import axios from "axios";
import authHeader from "./auth-header";


const getPublicContent =async () => {
    return await axios.get("/api/user/all");
};
const getUserBoard = async () => {
    let token = localStorage.getItem("token");
    return await axios.get("/api/user/user", { headers: authHeader(token!) });
};
const getModeratorBoard = async () => {
    let token = localStorage.getItem("token");
    return await axios.get("/api/user/mod", { headers: authHeader(token!) });
};
const getAdminBoard = async () => {
    let token = localStorage.getItem("token");
    return await axios.get("/api/user/admin", { headers: authHeader(token!) });
};
const UserService = {
    getPublicContent,
    getUserBoard,
    getModeratorBoard,
    getAdminBoard,
}
export default UserService;