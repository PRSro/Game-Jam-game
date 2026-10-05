import { createRouter, createWebHistory } from "vue-router";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: "/", redirect: "/evenimente" },
    {
      path: "/evenimente",
      name: "evenimente",
      component: () => import("../pages/EventsPage.vue"),
    },
    {
      path: "/calendar",
      name: "calendar",
      component: () => import("../pages/CalendarPage.vue"),
    },
    {
      path: "/harta",
      name: "harta",
      component: () => import("../pages/BucharestMap.vue"),
    },
    {
      path: "/design",
      name: "design",
      component: () => import("../pages/StyleguidePage.vue"),
    },
    {
      path: "/:pathMatch(.*)*",
      name: "not-found",
      component: () => import("../pages/NotFoundPage.vue"),
    },
  ],
  scrollBehavior: () => ({ top: 0 }),
});

export default router;
